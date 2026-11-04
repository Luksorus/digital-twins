using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class RuntimeVerification
{
    static List<string> results=new List<string>();
    static double startReal,pauseReal;
    static float pausedTime;
    static bool spawningChecked,paused,resumed,arrivalCaptured,departureCaptured,done;
    static RuntimeVerification(){EditorApplication.update+=Tick;EditorApplication.playModeStateChanged+=StateChanged;}
    public static void Begin()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Practical06_07.unity");
        SessionState.SetString("CityRuntimeResults","");SessionState.SetBool("CityRuntimeVerification",true);EditorApplication.EnterPlaymode();
    }
    static void Record(bool ok,string name){results.Add((ok?"PASS ":"FAIL ")+name);SessionState.SetString("CityRuntimeResults",string.Join("\n",results));if(!ok)throw new Exception(name);}
    static void StateChanged(PlayModeStateChange state)
    {
        if(!SessionState.GetBool("CityRuntimeVerification",false))return;
        if(state==PlayModeStateChange.EnteredPlayMode)startReal=EditorApplication.timeSinceStartup;
        if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool("CityRuntimeVerification",false);string result=SessionState.GetString("CityRuntimeResults","");File.WriteAllText(Path.Combine(CityBuilder.Root,"runtime-verification.txt"),result+"\n");Debug.Log("RUNTIME_VERIFICATION_FINISHED");if(Application.isBatchMode)EditorApplication.Exit(result.Contains("FAIL")||string.IsNullOrEmpty(result)?1:0);}
    }
    static void Tick()
    {
        if(!SessionState.GetBool("CityRuntimeVerification",false)||!EditorApplication.isPlaying||done)return;
        try{
            var sim=UnityEngine.Object.FindFirstObjectByType<CitySimulation>();if(!sim)return;
            if(!spawningChecked&&sim.elapsed>1){Record(sim.cars.Count==8,"Play Mode Start instantiates eight cars");Record(sim.cars[0].distance>.1f,"FixedUpdate advances real car positions");Record(sim.train.tripTime>.5f,"FixedUpdate advances train time");spawningChecked=true;}
            if(!paused&&sim.elapsed>3){sim.paused=true;pausedTime=sim.elapsed;pauseReal=EditorApplication.timeSinceStartup;paused=true;}
            if(paused&&!resumed&&EditorApplication.timeSinceStartup-pauseReal>.4){Record(Mathf.Abs(sim.elapsed-pausedTime)<.001f,"Pause holds simulation time in Play Mode");sim.paused=false;resumed=true;}
            var camera=GameObject.Find("Camera Station").GetComponent<Camera>();
            if(!arrivalCaptured&&sim.train.phase==StationTrain.Phase.Dwell){Record(sim.train.speed==0&&Mathf.Abs(sim.train.positionX-18)<.001f,"Train really stops at platform in Play Mode");CityBuilder.Capture(camera,"11_playmode_arrival");arrivalCaptured=true;}
            if(!departureCaptured&&sim.train.phase==StationTrain.Phase.Depart&&sim.train.speed>4){Record(sim.train.departures>=1,"Train departs after dwell in Play Mode");CityBuilder.Capture(camera,"12_playmode_departure");departureCaptured=true;}
            if(sim.elapsed>27&&departureCaptured){done=true;EditorApplication.ExitPlaymode();}
            if(EditorApplication.timeSinceStartup-startReal>70)throw new Exception("Play Mode verification timeout");
        }catch(Exception e){results.Add("FAIL "+e.Message);SessionState.SetString("CityRuntimeResults",string.Join("\n",results));Debug.LogException(e);done=true;EditorApplication.ExitPlaymode();}
    }
}
