using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class RoomBuilder
{
    public static readonly Vector3 Origin=new Vector3(160,0,0);
    public static GameObject room;
    public static Camera roomCamera;
    public static RoomLightSwitch lightSwitch;
    public static GameObject Table(string name,Vector3 p,bool chair)
    {
        var root=new GameObject(name);root.transform.position=Origin+p;
        void Part(string n,Vector3 at,Vector3 size,Color c){var g=CityBuilder.Box(n,Origin+p+at,size,c);g.transform.SetParent(root.transform,true);}
        Color wood=new Color(.57f,.32f,.15f);float h=chair?.48f:.86f,w=chair?.65f:2.8f,d=chair?.65f:1.45f;
        Part(name+"_Top",new Vector3(0,h,0),new Vector3(w,.12f,d),wood);
        foreach(float x in new[]{-w/2+.1f,w/2-.1f})foreach(float z in new[]{-d/2+.1f,d/2-.1f})Part(name+"_Leg",new Vector3(x,h/2,z),new Vector3(.1f,h,.1f),wood);
        if(chair)Part(name+"_Back",new Vector3(0,.95f,.28f),new Vector3(.65f,.9f,.12f),wood);
        var rb=root.AddComponent<Rigidbody>();rb.mass=chair?6:20;rb.linearDamping=1;rb.angularDamping=3;rb.interpolation=RigidbodyInterpolation.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
        return root;
    }
    public static GameObject Box(string name,Vector3 p,Vector3 size,Color c,bool collider=true){var g=CityBuilder.Box(name,Origin+p,size,c,null,collider);g.transform.SetParent(room.transform,true);return g;}
    public static PortableItem Item(string name,Vector3 p,Vector3 size,Color c,float mass=.35f)
    {
        var g=Box(name,p,size,c);var rb=g.AddComponent<Rigidbody>();rb.mass=mass;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;rb.interpolation=RigidbodyInterpolation.Interpolate;rb.linearDamping=.2f;
        var item=g.AddComponent<PortableItem>();item.label=name;return item;
    }
    static PortalDoor Portal(string name,Vector3 p,Color color,Transform destination)
    {
        var root=new GameObject(name);root.transform.position=p;
        foreach(float x in new[]{-1.1f,1.1f}){var bar=CityBuilder.Box(name+"_Frame",p+new Vector3(x,1.5f,0),new Vector3(.2f,3,.3f),color);bar.transform.SetParent(root.transform,true);}
        var top=CityBuilder.Box(name+"_Header",p+Vector3.up*3,new Vector3(2.4f,.2f,.3f),color);top.transform.SetParent(root.transform,true);
        var trigger=root.AddComponent<BoxCollider>();trigger.center=Vector3.up*1.35f;trigger.size=new Vector3(1.9f,2.6f,.55f);trigger.isTrigger=true;
        var rb=root.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;
        var portal=root.AddComponent<PortalDoor>();portal.destination=destination;
        CityBuilder.Text(name.ToUpper(),p+new Vector3(-1.1f,3.4f,0),.12f,(name=="TO ROOM"?Quaternion.identity:Quaternion.Euler(0,180,0)));return portal;
    }
    public static void Configure()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Practical06_07.unity");
        room=new GameObject("Interactive Room");CityBuilder.city=room;
        Color wall=new Color(.73f,.78f,.76f),trim=new Color(.12f,.25f,.28f);
        Box("RoomFloor",new Vector3(0,-.15f,0),new Vector3(14,.3f,12),new Color(.43f,.32f,.23f));
        Box("RoomRoof",new Vector3(0,4.2f,0),new Vector3(14,.25f,12),wall);
        Box("WallNorth",new Vector3(0,2.1f,6),new Vector3(14,4.2f,.25f),wall);
        Box("WallWest",new Vector3(-7,2.1f,0),new Vector3(.25f,4.2f,12),wall);
        Box("WallEast",new Vector3(7,2.1f,0),new Vector3(.25f,4.2f,12),wall);
        foreach(float x in new[]{-4.1f,4.1f})Box("WallSouth",new Vector3(x,2.1f,-6),new Vector3(5.8f,4.2f,.25f),wall);
        Box("DoorLintel",new Vector3(0,3.65f,-6),new Vector3(2.4f,1.1f,.25f),wall);
        for(int i=0;i<14;i++)Box("FloorSeam",new Vector3(i-6.5f,.006f,0),new Vector3(.014f,.01f,12),new Color(.26f,.19f,.14f),false);
        Table("WorkTable",new Vector3(-2,0,0),false);Table("SideTable",new Vector3(3.3f,0,.8f),false);
        foreach(var p in new[]{new Vector3(-2,0,-1.4f),new Vector3(-2,0,1.4f),new Vector3(3.3f,0,-.6f),new Vector3(4.8f,0,.8f)})Table("Chair",p,true);
        foreach(float x in new[]{-4f,1.5f}){
            Box("CabinetBack",new Vector3(x,1.35f,5.65f),new Vector3(2.8f,2.7f,.16f),trim);
            foreach(float side in new[]{-1.4f,1.4f})Box("CabinetSide",new Vector3(x+side,1.35f,5.15f),new Vector3(.15f,2.7f,1.1f),trim);
            foreach(float y in new[]{.15f,1.05f,1.95f,2.7f})Box("CabinetShelf",new Vector3(x,y,5.15f),new Vector3(2.8f,.12f,1.1f),trim);
            Item("CabinetBox",new Vector3(x,1.32f,5.08f),new Vector3(.55f,.42f,.45f),new Color(.8f,.42f,.15f));
            Item("CabinetBook",new Vector3(x+.8f,2.1f,5.08f),new Vector3(.3f,.2f,.48f),new Color(.18f,.5f,.7f));
        }
        Item("RedCube",new Vector3(-2,.12f+.93f,0),Vector3.one*.24f,new Color(.85f,.16f,.08f));
        Item("BlueBlock",new Vector3(-2.8f,1.1f,.2f),new Vector3(.3f,.3f,.22f),new Color(.1f,.4f,.8f));
        Item("Notebook",new Vector3(3.3f,.98f,.8f),new Vector3(.6f,.08f,.45f),new Color(.95f,.7f,.15f));
        Item("FloorParcel",new Vector3(-4,.25f,-2.8f),new Vector3(.55f,.5f,.45f),new Color(.7f,.55f,.32f));
        Item("FloorToy",new Vector3(2,.16f,-3),Vector3.one*.3f,new Color(.2f,.7f,.4f));
        var lamps=new System.Collections.Generic.List<Light>();
        foreach(float x in new[]{-3.5f,3.5f})foreach(float z in new[]{-2.5f,2.5f}){
            Box("CeilingPanel",new Vector3(x,4.02f,z),new Vector3(1,.08f,.65f),Color.white,false);
            var l=new GameObject("Room Light").AddComponent<Light>();l.transform.position=Origin+new Vector3(x,3.7f,z);l.type=LightType.Point;l.range=10;l.intensity=2.5f;l.color=new Color(1,.9f,.75f);l.shadows=LightShadows.Soft;lamps.Add(l);
        }
        var plate=Box("LightSwitch",new Vector3(-6.8f,1.4f,-3.5f),new Vector3(.12f,.32f,.22f),Color.white);
        lightSwitch=plate.AddComponent<RoomLightSwitch>();lightSwitch.lights=lamps.ToArray();
        var rocker=Box("SwitchRocker",new Vector3(-6.71f,1.4f,-3.5f),new Vector3(.07f,.2f,.14f),trim,false);rocker.transform.SetParent(plate.transform,true);lightSwitch.rocker=rocker.transform;lightSwitch.Set(true);
        var enter=new GameObject("Room Arrival").transform;enter.position=Origin+new Vector3(0,.05f,-3.7f);
        var exit=new GameObject("City Arrival").transform;exit.position=new Vector3(-23,.2f,-30.5f);exit.rotation=Quaternion.identity;
        Portal("TO ROOM",new Vector3(-23,.15f,-26.5f),new Color(.06f,.7f,.8f),enter);
        Portal("TO CITY",Origin+new Vector3(0,0,-5.6f),new Color(.9f,.52f,.1f),exit);
        var walker=UnityEngine.Object.FindFirstObjectByType<CityWalker>();walker.transform.position=new Vector3(-23,.15f,-30.3f);walker.transform.rotation=Quaternion.identity;
        var hand=walker.gameObject.AddComponent<RoomInteractor>();hand.eyes=walker.GetComponentInChildren<Camera>();
        roomCamera=CityBuilder.Camera("Camera Room",Origin+new Vector3(5.9f,3.3f,-4.9f),Origin+new Vector3(-1,1.2f,1.8f));roomCamera.fieldOfView=65;
        var rig=UnityEngine.Object.FindFirstObjectByType<CityCameraRig>();rig.cameras=rig.cameras.Concat(new[]{roomCamera}).ToArray();rig.Select(1);
        Physics.SyncTransforms();
    }
    public static void Capture(string name,Vector3 local,Vector3 target)
    {roomCamera.transform.position=Origin+local;roomCamera.transform.LookAt(Origin+target);CityBuilder.Capture(roomCamera,name);}
    [MenuItem("Practicals/Build room and evidence")]
    public static void Build()
    {
        try{
            System.IO.Directory.CreateDirectory(CityBuilder.Images);CityBuilder.checks.Clear();Configure();
            CityBuilder.Check(UnityEngine.Object.FindObjectsByType<PortalDoor>(FindObjectsSortMode.None).Length==2,"Two trigger portals link city and room");
            CityBuilder.Check(UnityEngine.Object.FindObjectsByType<PortableItem>(FindObjectsSortMode.None).Length==9,"Nine portable items on floor tables and shelves");
            CityBuilder.Check(GameObject.Find("WorkTable").GetComponent<Rigidbody>().mass==20,"Tables and chairs use dynamic compound colliders");
            Capture("01_room_overview",new Vector3(5.9f,3.3f,-4.9f),new Vector3(-1,1.2f,1.8f));
            Capture("02_tables",new Vector3(-5.8f,2.5f,-3.8f),new Vector3(-.3f,.9f,.7f));
            Capture("03_cabinets",new Vector3(-1,2.5f,1.8f),new Vector3(-1,1.5f,5.3f));
            Capture("04_floor_items",new Vector3(4,2,-4.9f),new Vector3(0,.2f,-2.7f));
            Capture("05_switch_on",new Vector3(-4.6f,1.8f,-4.7f),new Vector3(-6.8f,1.4f,-3.5f));
            Capture("06_lights_on",new Vector3(5.9f,3.3f,-4.9f),new Vector3(-1,1.2f,1.8f));
            lightSwitch.Set(false);CityBuilder.Check(lightSwitch.lights.All(l=>!l.enabled),"Wall switch disables every room light");
            Capture("07_lights_off",new Vector3(5.9f,3.3f,-4.9f),new Vector3(-1,1.2f,1.8f));lightSwitch.Set(true);
            Capture("08_return_portal",new Vector3(3,2.2f,-2),new Vector3(0,1.5f,-5.6f));
            var cam=CityBuilder.Camera("Portal detail",new Vector3(-29,3,-33),new Vector3(-23,1.5f,-26.5f));CityBuilder.Capture(cam,"09_city_portal");UnityEngine.Object.DestroyImmediate(cam.gameObject);
            string scene="Assets/Scenes/Practical08_09.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene,true)};AssetDatabase.SaveAssets();
            File.WriteAllLines(Path.Combine(CityBuilder.Root,"verification.txt"),CityBuilder.checks);Debug.Log("ROOM_BUILD_SUCCESS");if(Application.isBatchMode)EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);}
    }
}

