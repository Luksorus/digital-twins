using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class ConstructionRuntimeVerification
{
    static int stage,loaded,captured;static float since,stonesBefore,loadBefore;static Vector3 truckBefore;static double start;static string results="";
    static List<string> telemetry=new List<string>();
    static ConstructionRuntimeVerification(){EditorApplication.update+=Tick;EditorApplication.playModeStateChanged+=State;}
    public static void Begin(){EditorSceneManager.OpenScene("Assets/Scenes/Practical12_16.unity");SessionState.SetBool("ConstructionTest",true);SessionState.SetString("ConstructionResults","");SessionState.SetString("ConstructionTelemetry","");EditorApplication.EnterPlaymode();}
    static void Record(bool ok,string text){results+=(ok?"PASS ":"FAIL ")+text+"\n";SessionState.SetString("ConstructionResults",results);if(!ok)throw new Exception(text);}
    static void State(PlayModeStateChange state)
    {
        if(!SessionState.GetBool("ConstructionTest",false))return;
        if(state==PlayModeStateChange.EnteredPlayMode){start=EditorApplication.timeSinceStartup;since=Time.time;stage=0;telemetry=new List<string>{"stage,time,captured_grains,truck_grains,stone_mean_z,crane_load_y"};}
        if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool("ConstructionTest",false);string text=SessionState.GetString("ConstructionResults","");File.WriteAllText(Path.Combine(CityBuilder.Root,"runtime-verification.txt"),text);File.WriteAllText(Path.Combine(CityBuilder.Root,"telemetry.csv"),SessionState.GetString("ConstructionTelemetry",""));Debug.Log("CONSTRUCTION_RUNTIME_FINISHED");EditorApplication.Exit(text.Contains("FAIL")||text.Length==0?1:0);}
    }
    static ConstructionGrain[] Grains=>UnityEngine.Object.FindObjectsByType<ConstructionGrain>(FindObjectsSortMode.None);
    static int InTruck(ConstructionMachine truck)=>Grains.Count(g=>{Vector3 p=truck.bed.InverseTransformPoint(g.transform.position);return p.x> -1.5f&&p.x<1.5f&&p.y>.1f&&p.y<1.6f&&p.z>.1f&&p.z<5.9f;});
    static float Stones()=>Grains.Where(g=>g.materialKind=="Stone").Average(g=>g.transform.position.z);
    static void Next(ConstructionControls controls)
    {
        telemetry.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0},{1:F2},{2},{3},{4:F3},{5:F3}",stage,Time.time,controls.machines[1].bucket.contents.Count,InTruck(controls.machines[2]),Stones(),GameObject.Find("Crane Load").transform.position.y));SessionState.SetString("ConstructionTelemetry",string.Join("\n",telemetry)+"\n");stage++;since=Time.time;
    }
    static void Capture(string name,Vector3 pos,Vector3 target)
    {ConstructionBuilder.siteCamera=GameObject.Find("Camera Site").GetComponent<Camera>();ConstructionBuilder.Capture(name,pos,target);}
    static void Tick()
    {
        if(!SessionState.GetBool("ConstructionTest",false)||!EditorApplication.isPlaying)return;
        try{
            if(EditorApplication.timeSinceStartup-start>120)throw new Exception("Construction timeout stage "+stage);
            var controls=UnityEngine.Object.FindFirstObjectByType<ConstructionControls>();if(!controls)return;controls.automaticTest=true;
            var dozer=controls.machines[0];var excavator=controls.machines[1];var truck=controls.machines[2];var crane=controls.machines[3].hoist;float t=Time.time-since;
            switch(stage){
            case 0: if(t<2)return;Record(Grains.Length==92,"All ninety-two physical grains persist after settling");Capture("09_playmode_bucket_fill",new Vector3(1,4,12),new Vector3(-3,.7f,7));captured=excavator.bucket.Collect();Record(captured>=20,"Bucket captures at least twenty overlapping sand grains without spawning new cargo");Record(excavator.bucket.contents.All(b=>b.isKinematic&&b.transform.parent==excavator.bucket.transform),"Bucket retainer holds only collected Rigidbody objects");Next(controls);break;
            case 1: float a=Mathf.Clamp01(t/2);excavator.boomAngle=Mathf.Lerp(-25,-55,a);excavator.stickAngle=Mathf.Lerp(85,50,a);excavator.bucketAngle=-(excavator.boomAngle+excavator.stickAngle);if(t<2.5f)return;Record(excavator.bucket.transform.position.y>4,"Articulated boom lifts captured grains above truck height");Capture("10_playmode_bucket_lift",new Vector3(-14,9,12),new Vector3(-2,3,4));Next(controls);break;
            case 2: excavator.yaw=Mathf.Lerp(0,90,Mathf.Clamp01(t/3));if(t<3.5f)return;Record(excavator.bucket.transform.position.x>2,"Turret swings physical bucket above truck bed");Capture("11_playmode_above_truck",new Vector3(13,9,10),new Vector3(2,3,1));excavator.bucket.Release();Record(excavator.bucket.contents.Count==0,"Releasing bucket restores gravity to captured grains");Next(controls);break;
            case 3: excavator.bucketAngle=Mathf.Lerp(5,75,Mathf.Clamp01(t/1.5f));if(t<5)return;loaded=InTruck(truck);Debug.Log("TRUCK LOADED "+loaded);Record(loaded>=15,"At least fifteen released sand grains settle inside collider truck bed");Capture("12_playmode_truck_loaded",new Vector3(10,7,-5),new Vector3(3.2f,2,1));truckBefore=truck.transform.position;truck.drive=.5f;Next(controls);break;
            case 4: if(t<2)return;truck.drive=0;Record(Vector3.Distance(truckBefore,truck.transform.position)>1,"Truck drives with keyboard-equivalent drive command in FixedUpdate");Record(InTruck(truck)>=loaded*.6f,"Physical truck bed retains most cargo during transport");Capture("13_playmode_transport",new Vector3(13,8,-7),new Vector3(3.2f,2,3));stonesBefore=Stones();dozer.drive=.65f;Next(controls);break;
            case 5: if(t<3.5f)return;dozer.drive=0;Record(Stones()>stonesBefore+.6f,"Bulldozer blade physically pushes stone pile forward");Capture("14_playmode_dozer_push",new Vector3(-11,7,-14),new Vector3(-20,.7f,-3));crane.Toggle();Record(crane.joint,"Crane hook connects nearby dynamic load through FixedJoint");loadBefore=GameObject.Find("Crane Load").transform.position.y;Next(controls);break;
            case 6: crane.drop=Mathf.Lerp(10.5f,6,Mathf.Clamp01(t/3));if(t<3.5f)return;crane.yaw=Mathf.Lerp(0,30,Mathf.Clamp01((t-3.5f)/2));if(t<5.8f)return;Record(GameObject.Find("Crane Load").transform.position.y>loadBefore+3,"Crane winch physically lifts joint-connected cargo");Capture("15_playmode_crane_lift",new Vector3(37,13,-9),new Vector3(27,6,3));crane.Toggle();Record(!crane.joint&&!GameObject.Find("Crane Load").GetComponent<Rigidbody>().isKinematic,"Crane releases cargo back to free dynamic motion");Next(controls);break;
            case 7: if(t<3)return;Record(GameObject.Find("Crane Load").transform.position.y<1.5f,"Released crane load lands on site floor");Capture("16_playmode_crane_release",new Vector3(37,13,-9),new Vector3(27,5,3));truck.dumping=true;Next(controls);break;
            case 8: if(t<8)return;Record(truck.bedAngle>44,"Truck tipper rotates to forty-five degrees");Record(InTruck(truck)<loaded*.5f,"Tilted bed and open tailgate unload physical sand grains");Capture("17_playmode_truck_dump",new Vector3(13,8,-7),new Vector3(3.2f,2,3));Next(controls);break;
            case 9: EditorApplication.ExitPlaymode();stage++;break;
            }
        }catch(Exception e){results+="FAIL "+e.Message+"\n";SessionState.SetString("ConstructionResults",results);Debug.LogException(e);stage=99;EditorApplication.ExitPlaymode();}
    }
}
