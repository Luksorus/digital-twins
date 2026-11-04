using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class MechanismRuntimeVerification
{
    static int stage;static float since;static double start;static string results="";static Vector3 before;
    static MechanismRuntimeVerification(){EditorApplication.update+=Tick;EditorApplication.playModeStateChanged+=State;}
    public static void Begin(){EditorSceneManager.OpenScene("Assets/Scenes/Practical10_11.unity");SessionState.SetBool("MechanismTest",true);SessionState.SetString("MechanismResults","");EditorApplication.EnterPlaymode();}
    static void Record(bool ok,string text){results+=(ok?"PASS ":"FAIL ")+text+"\n";SessionState.SetString("MechanismResults",results);if(!ok)throw new Exception(text);}
    static void State(PlayModeStateChange state)
    {
        if(!SessionState.GetBool("MechanismTest",false))return;
        if(state==PlayModeStateChange.EnteredPlayMode){start=EditorApplication.timeSinceStartup;stage=0;since=Time.time;}
        if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool("MechanismTest",false);string text=SessionState.GetString("MechanismResults","");File.WriteAllText(Path.Combine(CityBuilder.Root,"runtime-verification.txt"),text);Debug.Log("MECHANISM_RUNTIME_FINISHED");EditorApplication.Exit(text.Contains("FAIL")||text.Length==0?1:0);}
    }
    static void Next(){stage++;since=Time.time;}
    static void Tick()
    {
        if(!SessionState.GetBool("MechanismTest",false)||!EditorApplication.isPlaying)return;
        try{
            if(EditorApplication.timeSinceStartup-start>70)throw new Exception("Mechanism timeout stage "+stage);
            var hands=UnityEngine.Object.FindFirstObjectByType<RoomInteractor>();if(!hands||Time.time<.5f)return;
            var controls=hands.GetComponent<RoomMechanisms>();var item=UnityEngine.Object.FindObjectsByType<CabinetItem>(FindObjectsSortMode.None).OrderBy(i=>i.transform.position.x).First();var door=item.door;
            float t=Time.time-since;
            switch(stage){
            case 0:
                var cc=hands.GetComponent<CharacterController>();cc.enabled=false;cc.transform.position=RoomBuilder.Origin+new Vector3(-2.8f,.1f,2f);cc.transform.rotation=Quaternion.identity;cc.enabled=true;
                hands.eyes.transform.LookAt(item.transform.position);Physics.SyncTransforms();
                Record(!hands.Take(item)&&!item.CanTake,"Closed cabinet rejects item pickup in Play Mode");
                Record(Physics.Raycast(hands.eyes.transform.position,(item.transform.position-hands.eyes.transform.position).normalized,out var hit,3.5f)&&hit.collider.GetComponentInParent<HingedDoor>(),"Closed door collider physically occludes shelf item");
                Record(controls.UseDoor(),"Keyboard door action uses camera ray to select door");Next();break;
            case 1: if(t<1.5f)return;Record(door.IsOpen&&door.angle>100,"FixedUpdate rotates cabinet door to open position");hands.eyes.transform.LookAt(item.transform.position);Physics.SyncTransforms();bool taken=hands.Take(item);if(!taken){Vector3 d=item.transform.position-hands.eyes.transform.position;Physics.Raycast(hands.eyes.transform.position,d.normalized,out var obstruction,d.magnitude+.05f,~0,QueryTriggerInteraction.Ignore);Debug.Log("PICKUP DEBUG item="+item.name+" pos="+item.transform.position+" eyes="+hands.eyes.transform.position+" allowed="+item.CanTake+" obstruction="+(obstruction.collider?obstruction.collider.name:"none"));}Record(taken,"Opening cabinet permits visible item pickup");CityBuilder.Capture(hands.eyes,"09_playmode_cabinet_item");hands.Drop();door.Toggle();Next();break;
            case 2: if(t<1.5f)return;Record(door.angle<1&&!door.IsOpen,"Cabinet door closes back to zero degrees");
                var entry=GameObject.Find("Entrance Door").GetComponent<HingedDoor>();entry.Toggle();Next();break;
            case 3: if(t<1.5f)return;Record(GameObject.Find("Entrance Door").GetComponent<HingedDoor>().IsOpen,"Entrance door opens with its physical collider");
                before=GameObject.Find("RedCube").transform.position;
                var camera=GameObject.Find("Camera Room").GetComponent<Camera>();camera.transform.position=RoomBuilder.Origin+new Vector3(-.5f,2.2f,-2.6f);camera.transform.LookAt(RoomBuilder.Origin+new Vector3(-2.2f,1.2f,0));CityBuilder.Capture(camera,"10_playmode_fan_before");controls.fan.Toggle();Next();break;
            case 4: if(t<5)return;var red=GameObject.Find("RedCube");Record(red.transform.position.x>before.x+1.4f,"Fan force blows lightweight item past table edge");Record(red.transform.position.y<.5f&&GameObject.Find("BlueBlock").transform.position.y<.5f,"Both lightweight items fall from table under gravity");Record(controls.fan.rotation>0,"Fan rotor rotates while wind is active");
                var fanCamera=GameObject.Find("Camera Room").GetComponent<Camera>();CityBuilder.Capture(fanCamera,"11_playmode_fan_after");controls.fan.Toggle();controls.blinds.Toggle();Next();break;
            case 5: if(t<1.2f)return;Record(controls.blinds.openness>.99f,"Blinds lift to fully open in FixedUpdate");
                var blindCamera=GameObject.Find("Camera Room").GetComponent<Camera>();blindCamera.transform.position=RoomBuilder.Origin+new Vector3(2.5f,2.6f,-2);blindCamera.transform.LookAt(RoomBuilder.Origin+new Vector3(6.8f,2.1f,0));CityBuilder.Capture(blindCamera,"12_playmode_blinds");controls.blinds.Toggle();Next();break;
            case 6: if(t<1.2f)return;Record(controls.blinds.openness<.01f,"Blinds close again in FixedUpdate");EditorApplication.ExitPlaymode();stage++;break;
            }
        }catch(Exception e){results+="FAIL "+e.Message+"\n";SessionState.SetString("MechanismResults",results);Debug.LogException(e);stage=99;EditorApplication.ExitPlaymode();}
    }
}


