using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class RoomRuntimeVerification
{
    static int stage;static double since,start;static Vector3 itemStart,tableStart;static PortableItem item;static string results="";
    static RoomRuntimeVerification(){EditorApplication.update+=Tick;EditorApplication.playModeStateChanged+=State;}
    public static void Begin(){EditorSceneManager.OpenScene("Assets/Scenes/Practical08_09.unity");SessionState.SetBool("RoomTest",true);SessionState.SetString("RoomResults","");EditorApplication.EnterPlaymode();}
    static void Record(bool ok,string text){results+=(ok?"PASS ":"FAIL ")+text+"\n";SessionState.SetString("RoomResults",results);if(!ok)throw new Exception(text);}
    static void State(PlayModeStateChange state)
    {
        if(!SessionState.GetBool("RoomTest",false))return;
        if(state==PlayModeStateChange.EnteredPlayMode){stage=0;start=since=EditorApplication.timeSinceStartup;PortalDoor.lastTeleport=-100;}
        if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool("RoomTest",false);var text=SessionState.GetString("RoomResults","");File.WriteAllText(Path.Combine(CityBuilder.Root,"runtime-verification.txt"),text);Debug.Log("ROOM_RUNTIME_FINISHED");EditorApplication.Exit(text.Contains("FAIL")||text.Length==0?1:0);}
    }
    static void Place(CharacterController c,Vector3 p,Quaternion rotation){c.enabled=false;c.transform.SetPositionAndRotation(p,rotation);c.enabled=true;Physics.SyncTransforms();}
    static void Next(){stage++;since=EditorApplication.timeSinceStartup;}
    static void Tick()
    {
        if(!SessionState.GetBool("RoomTest",false)||!EditorApplication.isPlaying)return;
        try{
            double t=EditorApplication.timeSinceStartup-since;if(EditorApplication.timeSinceStartup-start>45)throw new Exception("Room test timeout stage "+stage+" position "+UnityEngine.Object.FindFirstObjectByType<CityWalker>().transform.position);
            var hands=UnityEngine.Object.FindFirstObjectByType<RoomInteractor>();if(!hands||Time.time<.5f)return;var c=hands.GetComponent<CharacterController>();
            switch(stage){
            case 0: Record(UnityEngine.Object.FindFirstObjectByType<CitySimulation>().cars.Count==8,"Inherited city spawns eight cars in Play Mode");Place(c,new Vector3(-23,.16f,-27.5f),Quaternion.identity);Next();break;
            case 1: if(c.transform.position.z< -26.55f)c.Move(Vector3.forward*.015f);if(c.transform.position.x>100){Record(true,"CharacterController entering city trigger teleports into room");Record(!GameObject.Find("TO CITY").GetComponent<PortalDoor>().Enter(c),"Portal cooldown blocks immediate return");CityBuilder.Capture(hands.eyes,"10_playmode_room_arrival");Next();}break;
            case 2: if(t<.5)return;var sw=UnityEngine.Object.FindFirstObjectByType<RoomLightSwitch>();bool was=sw.isOn;sw.Toggle();Record(sw.isOn!=was&&!sw.lights[0].enabled,"Wall switch toggles actual room lights in Play Mode");sw.Toggle();
                item=GameObject.Find("RedCube").GetComponent<PortableItem>();Place(c,RoomBuilder.Origin+new Vector3(-.6f,.1f,-1.7f),Quaternion.identity);Vector3 aim=item.transform.position-hands.eyes.transform.position;c.transform.rotation=Quaternion.LookRotation(new Vector3(aim.x,0,aim.z));hands.eyes.transform.LookAt(item.transform.position);Physics.SyncTransforms();Record(hands.Interact()&&hands.held,"Ray interaction picks up visible small object");itemStart=item.transform.position;Next();break;
            case 3: c.Move(Vector3.right*.02f);if(t>.6){Record(Vector3.Distance(itemStart,item.transform.position)>.3f&&hands.held.isKinematic,"Carried object follows the camera");CityBuilder.Capture(hands.eyes,"11_playmode_carry");hands.Drop();Record(!item.GetComponent<Rigidbody>().isKinematic,"Release restores dynamic Rigidbody");tableStart=GameObject.Find("WorkTable").transform.position;GameObject.Find("WorkTable").GetComponent<Rigidbody>().AddForce(Vector3.left*40,ForceMode.Impulse);Next();}break;
            case 4: if(t<1)return;Record(GameObject.Find("WorkTable").transform.position.x<tableStart.x-.05f,"Furniture responds to impulse through dynamic physics");
                Place(c,RoomBuilder.Origin+new Vector3(0,.1f,-4.5f),Quaternion.Euler(0,180,0));hands.eyes.transform.localRotation=Quaternion.identity;Next();break;
            case 5: if(c.transform.position.z> -5.65f)c.Move(Vector3.back*.015f);if(c.transform.position.x<100){Record(true,"Room exit trigger returns walker to the city");CityBuilder.Capture(hands.eyes,"12_playmode_return_city");Next();}break;
            case 6: EditorApplication.ExitPlaymode();stage++;break;
            }
        }catch(Exception e){results+="FAIL "+e.Message+"\n";SessionState.SetString("RoomResults",results);Debug.LogException(e);stage=99;EditorApplication.ExitPlaymode();}
    }
}


