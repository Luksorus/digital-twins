using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class MechanismBuilder
{
    public static HingedDoor Door(string name,Vector3 local,float width,float height)
    {
        var root=new GameObject(name);root.transform.position=RoomBuilder.Origin+local;
        var leaf=CityBuilder.Box(name+"_Panel",root.transform.position+new Vector3(width/2,height/2,0),new Vector3(width,height,.1f),new Color(.28f,.4f,.42f));leaf.transform.SetParent(root.transform,true);
        var handle=CityBuilder.Box(name+"_Handle",root.transform.position+new Vector3(width-.18f,height*.48f,-.1f),new Vector3(.08f,.3f,.08f),new Color(.9f,.72f,.22f));handle.transform.SetParent(root.transform,true);
        var rb=root.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
        return root.AddComponent<HingedDoor>();
    }
    public static void Configure()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Practical08_09.unity");RoomBuilder.room=GameObject.Find("Interactive Room");CityBuilder.city=RoomBuilder.room;
        foreach(var x in new[]{-4f,1.5f}){
            var door=Door("Cabinet Door "+x,new Vector3(x-1.4f,0,4.5f),2.8f,2.7f);
            foreach(var item in UnityEngine.Object.FindObjectsByType<PortableItem>(FindObjectsSortMode.None).Where(i=>i.transform.position.z>4&&Mathf.Abs(i.transform.position.x-RoomBuilder.Origin.x-x)<1.4f).ToArray()){
                var g=item.gameObject;UnityEngine.Object.DestroyImmediate(item);var gate=g.AddComponent<CabinetItem>();gate.door=door;gate.label=g.name;
            }
        }
        Door("Entrance Door",new Vector3(-1.15f,0,-6),2.3f,3.1f);
        GameObject.Find("TO CITY").transform.position=RoomBuilder.Origin+new Vector3(0,0,-6.6f);
        // Replace the east wall with four sections around a real window opening.
        UnityEngine.Object.DestroyImmediate(GameObject.Find("WallEast"));Color wall=new Color(.73f,.78f,.76f);
        foreach(float z in new[]{-3.75f,3.75f})RoomBuilder.Box("WindowSideWall",new Vector3(7,2.1f,z),new Vector3(.25f,4.2f,4.5f),wall);
        RoomBuilder.Box("WindowLowWall",new Vector3(7,.55f,0),new Vector3(.25f,1.1f,3),wall);
        RoomBuilder.Box("WindowHighWall",new Vector3(7,3.75f,0),new Vector3(.25f,.9f,3),wall);
        var glass=RoomBuilder.Box("WindowGlass",new Vector3(7.05f,2.2f,0),new Vector3(.05f,2.2f,3),new Color(.42f,.72f,.87f));
        foreach(float y in new[]{1.1f,3.3f})RoomBuilder.Box("WindowFrame",new Vector3(6.88f,y,0),new Vector3(.2f,.1f,3.2f),Color.white);
        var blindsRoot=new GameObject("Window Blinds");blindsRoot.transform.position=RoomBuilder.Origin+new Vector3(6.7f,0,0);var blinds=blindsRoot.AddComponent<WindowBlinds>();
        blinds.slats=new Transform[11];for(int i=0;i<11;i++){var slat=RoomBuilder.Box("BlindSlat",new Vector3(6.7f,1.2f+i*.19f,0),new Vector3(.06f,.17f,2.9f),new Color(.91f,.86f,.67f));slat.transform.SetParent(blindsRoot.transform,true);blinds.slats[i]=slat.transform;}blinds.Step(0);
        var fanRoot=new GameObject("Table Fan");fanRoot.transform.position=RoomBuilder.Origin+new Vector3(-3.15f,1.35f,0);var fan=fanRoot.AddComponent<RoomFan>();
        var foot=RoomBuilder.Box("FanBase",new Vector3(-3.15f,.96f,0),new Vector3(.42f,.1f,.5f),new Color(.16f,.2f,.23f));
        RoomBuilder.Box("FanStand",new Vector3(-3.15f,1.15f,0),new Vector3(.1f,.4f,.1f),new Color(.2f,.25f,.3f));
        var rotor=new GameObject("Fan Rotor").transform;rotor.SetParent(fanRoot.transform,false);rotor.localPosition=Vector3.right*.12f;fan.rotor=rotor;
        for(int i=0;i<3;i++){var pivot=new GameObject("Blade Pivot").transform;pivot.SetParent(rotor,false);pivot.localRotation=Quaternion.Euler(i*120,0,0);var blade=CityBuilder.Box("FanBlade",Vector3.zero,new Vector3(.07f,.42f,.12f),new Color(.13f,.36f,.45f),null,false);blade.transform.SetParent(pivot,false);blade.transform.localPosition=Vector3.up*.2f;}
        var hub=RoomBuilder.Box("FanHub",new Vector3(-2.98f,1.35f,0),Vector3.one*.16f,new Color(.25f,.3f,.33f));
        var hands=UnityEngine.Object.FindFirstObjectByType<RoomInteractor>();var controls=hands.gameObject.AddComponent<RoomMechanisms>();controls.eyes=hands.eyes;controls.fan=fan;controls.blinds=blinds;
        foreach(string name in new[]{"RedCube","BlueBlock"})GameObject.Find(name).GetComponent<Rigidbody>().mass=.08f;
        GameObject.Find("BlueBlock").transform.position=RoomBuilder.Origin+new Vector3(-1.4f,1.1f,.5f);
        RoomBuilder.roomCamera=GameObject.Find("Camera Room").GetComponent<Camera>();Physics.SyncTransforms();
    }
    [MenuItem("Practicals/Build room mechanisms and evidence")]
    public static void Build()
    {
        try{
            Directory.CreateDirectory(CityBuilder.Images);CityBuilder.checks.Clear();Configure();
            var doors=UnityEngine.Object.FindObjectsByType<HingedDoor>(FindObjectsSortMode.None);
            CityBuilder.Check(doors.Length==3,"Two cabinet doors and one entrance door have collider bodies");
            CityBuilder.Check(UnityEngine.Object.FindObjectsByType<CabinetItem>(FindObjectsSortMode.None).All(i=>!i.CanTake),"Four cabinet items are inaccessible behind closed doors");
            RoomBuilder.Capture("01_room_mechanisms",new Vector3(5.9f,3.3f,-4.9f),new Vector3(-1,1.2f,1.8f));
            RoomBuilder.Capture("02_cabinets_closed",new Vector3(-1,2.5f,1.8f),new Vector3(-1,1.5f,5.3f));
            foreach(var door in doors){door.targetOpen=true;door.Step(2,true);}Physics.SyncTransforms();
            CityBuilder.Check(UnityEngine.Object.FindObjectsByType<CabinetItem>(FindObjectsSortMode.None).All(i=>i.CanTake),"Cabinet items become eligible after doors open");
            RoomBuilder.Capture("03_cabinets_open",new Vector3(-1,2.5f,1.8f),new Vector3(-1,1.5f,5.3f));
            RoomBuilder.Capture("04_entrance_open",new Vector3(3,2.2f,-2),new Vector3(0,1.5f,-5.8f));
            foreach(var door in doors){door.targetOpen=false;door.Step(2,true);}
            RoomBuilder.Capture("05_entrance_closed",new Vector3(3,2.2f,-2),new Vector3(0,1.5f,-5.8f));
            RoomBuilder.Capture("06_fan_and_items",new Vector3(-.5f,2.2f,-2.6f),new Vector3(-2.2f,1.2f,0));
            var blinds=UnityEngine.Object.FindFirstObjectByType<WindowBlinds>();RoomBuilder.Capture("07_blinds_closed",new Vector3(2.5f,2.6f,-2),new Vector3(6.8f,2.1f,0));
            blinds.targetOpen=true;blinds.Step(2);CityBuilder.Check(blinds.openness==1,"Eleven slats lift and rotate when blinds open");
            RoomBuilder.Capture("08_blinds_open",new Vector3(2.5f,2.6f,-2),new Vector3(6.8f,2.1f,0));blinds.targetOpen=false;blinds.Step(2);
            string scene="Assets/Scenes/Practical10_11.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene,true)};AssetDatabase.SaveAssets();File.WriteAllLines(Path.Combine(CityBuilder.Root,"verification.txt"),CityBuilder.checks);Debug.Log("MECHANISMS_BUILD_SUCCESS");if(Application.isBatchMode)EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);}
    }
}
