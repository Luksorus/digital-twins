using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class ConstructionBuilder
{
    public static Vector3 Origin=new Vector3(0,0,-78);
    public static Color yellow=new Color(.95f,.63f,.08f),dark=new Color(.13f,.16f,.18f),glass=new Color(.26f,.53f,.66f);
    public static Camera siteCamera;
    public static GameObject Part(string name,Transform parent,Vector3 pos,Vector3 size,Color color,bool collider=true)
    {
        var g=CityBuilder.Box(name,Vector3.zero,size,color,null,collider);g.transform.SetParent(parent,false);g.transform.localPosition=pos;return g;
    }
    static Transform Pivot(string name,Transform parent,Vector3 p)
    {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
    static void Beam(string name,Transform parent,Vector3 a,Vector3 b,float width,Color c)
    {var g=Part(name,parent,(a+b)/2,new Vector3(width,(b-a).magnitude,width),c,false);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
    static ConstructionMachine Machine(string name,ConstructionMachine.Kind kind,Vector3 p)
    {
        var root=new GameObject(name);root.transform.position=Origin+p;
        var rb=root.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;rb.interpolation=RigidbodyInterpolation.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
        var m=root.AddComponent<ConstructionMachine>();m.kind=kind;return m;
    }
    static void Tracks(Transform root)
    {
        foreach(float x in new[]{-1.15f,1.15f}){
            Part("Track",root,new Vector3(x,.45f,0),new Vector3(.65f,.75f,3.5f),dark);
            for(int i=0;i<5;i++){
                var wheel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);wheel.name="TrackWheel";UnityEngine.Object.DestroyImmediate(wheel.GetComponent<Collider>());wheel.transform.SetParent(root,false);wheel.transform.localPosition=new Vector3(x+(x>0?.34f:-.34f),.46f,-1.3f+i*.65f);wheel.transform.localRotation=Quaternion.Euler(0,0,90);wheel.transform.localScale=new Vector3(.52f,.05f,.52f);wheel.GetComponent<Renderer>().sharedMaterial=CityBuilder.Material("WheelSteel",new Color(.38f,.4f,.41f));
            }
        }
    }
    static void Cab(Transform parent,Vector3 pos,float size=1)
    {
        Part("CabFrame",parent,pos,new Vector3(1.5f,1.6f,1.5f)*size,yellow);
        Part("CabWindow",parent,pos+new Vector3(0,.15f,.76f)*size,new Vector3(1.25f,1,.025f)*size,glass,false);
        foreach(float x in new[]{-.76f,.76f})Part("CabSideGlass",parent,pos+new Vector3(x,.15f,0)*size,new Vector3(.025f,1,1.2f)*size,glass,false);
        Part("CabRoof",parent,pos+Vector3.up*.85f*size,new Vector3(1.7f,.15f,1.7f)*size,dark);
    }
    public static ConstructionMachine Dozer()
    {
        var m=Machine("Bulldozer",ConstructionMachine.Kind.Bulldozer,new Vector3(-20,0,-9));Tracks(m.transform);
        Part("DozerChassis",m.transform,new Vector3(0,.95f,0),new Vector3(2.2f,.7f,3.2f),yellow);Cab(m.transform,new Vector3(0,1.9f,-.6f));
        Part("DozerEngine",m.transform,new Vector3(0,1.3f,1),new Vector3(1.7f,.7f,1.3f),yellow);
        m.blade=Pivot("Dozer Blade",m.transform,new Vector3(0,.55f,2.2f));Part("BladePanel",m.blade,Vector3.zero,new Vector3(3.6f,1.1f,.25f),new Color(.62f,.65f,.67f));
        foreach(float x in new[]{-1f,1f})Part("BladeArm",m.transform,new Vector3(x,.6f,1.3f),new Vector3(.18f,.18f,2),yellow);
        return m;
    }
    public static ConstructionMachine Excavator()
    {
        var m=Machine("Excavator",ConstructionMachine.Kind.Excavator,new Vector3(-3,0,1));Tracks(m.transform);
        Part("ExcavatorBase",m.transform,new Vector3(0,1,0),new Vector3(2.4f,.6f,3.3f),yellow);
        m.turret=Pivot("Excavator Turret",m.transform,new Vector3(0,1.3f,0));Part("Counterweight",m.turret,new Vector3(0,.5f,-1),new Vector3(2.4f,1,1.7f),yellow);Cab(m.turret,new Vector3(-.8f,1.1f,.25f),.8f);
        m.boom=Pivot("Boom",m.turret,new Vector3(0,.6f,.2f));Part("BoomSteel",m.boom,new Vector3(0,0,2),new Vector3(.5f,.55f,4),yellow);
        m.stick=Pivot("Stick",m.boom,new Vector3(0,0,4));Part("StickSteel",m.stick,new Vector3(0,0,1.5f),new Vector3(.4f,.45f,3),yellow);
        m.bucketPivot=Pivot("Excavator Bucket",m.stick,new Vector3(0,0,3));m.bucket=m.bucketPivot.gameObject.AddComponent<ExcavatorBucket>();
        Part("BucketFloor",m.bucketPivot,new Vector3(0,-.65f,.7f),new Vector3(2,.14f,1.55f),dark);
        foreach(float x in new[]{-.95f,.95f})Part("BucketSide",m.bucketPivot,new Vector3(x,-.32f,.7f),new Vector3(.12f,.65f,1.55f),dark);
        Part("BucketBack",m.bucketPivot,new Vector3(0,-.35f,-.03f),new Vector3(1.9f,.6f,.12f),dark);
        for(int i=0;i<5;i++)Part("BucketTooth",m.bucketPivot,new Vector3(-.75f+i*.375f,-.63f,1.65f),new Vector3(.18f,.16f,.35f),new Color(.55f,.57f,.59f));
        m.Pose();return m;
    }
    public static ConstructionMachine Truck()
    {
        var m=Machine("Dump Truck",ConstructionMachine.Kind.Truck,new Vector3(3.2f,0,1));m.speed=2.5f;
        Part("TruckChassis",m.transform,new Vector3(0,.8f,.7f),new Vector3(2.8f,.45f,9),dark);Cab(m.transform,new Vector3(0,1.9f,3.9f),1.25f);
        foreach(float x in new[]{-1.5f,1.5f})foreach(float z in new[]{-2f,1.6f,4f}){
            var w=GameObject.CreatePrimitive(PrimitiveType.Cylinder);w.name="TruckWheel";w.transform.SetParent(m.transform,false);w.transform.localPosition=new Vector3(x,.6f,z);w.transform.localRotation=Quaternion.Euler(0,0,90);w.transform.localScale=new Vector3(1.2f,.22f,1.2f);UnityEngine.Object.DestroyImmediate(w.GetComponent<Collider>());w.GetComponent<Renderer>().sharedMaterial=CityBuilder.Material("TruckTire",dark);
        }
        m.bed=Pivot("Truck Bed",m.transform,new Vector3(0,1.5f,-3));
        Part("BedFloor",m.bed,new Vector3(0,0,3),new Vector3(3.3f,.2f,6),new Color(.7f,.25f,.1f));
        foreach(float x in new[]{-1.65f,1.65f})Part("BedSide",m.bed,new Vector3(x,.55f,3),new Vector3(.14f,1.2f,6),new Color(.7f,.25f,.1f));
        Part("BedFront",m.bed,new Vector3(0,.55f,6),new Vector3(3.3f,1.2f,.14f),new Color(.7f,.25f,.1f));
        m.tailgate=Pivot("Tailgate",m.bed,Vector3.zero);Part("TailgatePanel",m.tailgate,new Vector3(0,.55f,0),new Vector3(3.3f,1.2f,.14f),new Color(.7f,.25f,.1f));return m;
    }
    public static ConstructionMachine Crane()
    {
        var m=Machine("Tower Crane",ConstructionMachine.Kind.Crane,new Vector3(22,0,4));
        Part("CraneFoundation",m.transform,new Vector3(0,.3f,0),new Vector3(4,.6f,4),new Color(.5f,.52f,.53f));
        foreach(float x in new[]{-.7f,.7f})foreach(float z in new[]{-.7f,.7f})Part("TowerPost",m.transform,new Vector3(x,6,z),new Vector3(.15f,12,.15f),yellow);
        for(int i=0;i<8;i++)foreach(float x in new[]{-.7f,.7f})Beam("TowerBrace",m.transform,new Vector3(x,i*1.5f,-.7f),new Vector3(x,(i+1)*1.5f,.7f),.1f,yellow);
        var hoist=m.gameObject.AddComponent<CraneHoist>();m.hoist=hoist;hoist.slew=Pivot("Crane Slew",m.transform,Vector3.up*12);hoist.trolley=Pivot("Trolley",hoist.slew,Vector3.right*6);
        foreach(float z in new[]{-.55f,.55f})Part("CraneJib",hoist.slew,new Vector3(4,0,z),new Vector3(18,.18f,.15f),yellow);
        for(int i=-4;i<13;i++)Beam("JibBrace",hoist.slew,new Vector3(i,0,-.55f),new Vector3(i+1,0,.55f),.09f,yellow);
        Part("CraneCounterweight",hoist.slew,new Vector3(-4,-.4f,0),new Vector3(2,1.1f,2),new Color(.47f,.49f,.51f));
        Part("TrolleyBody",hoist.trolley,Vector3.zero,new Vector3(.7f,.5f,1.3f),dark);
        var hook=CityBuilder.Box("Crane Hook",Vector3.zero,new Vector3(.3f,.3f,.3f),dark);hoist.hook=hook.AddComponent<Rigidbody>();hoist.hook.isKinematic=true;hoist.hook.useGravity=false;
        hoist.cable=new GameObject("Hoist Cable").AddComponent<LineRenderer>();hoist.cable.startWidth=hoist.cable.endWidth=.045f;hoist.cable.sharedMaterial=CityBuilder.Material("Cable",dark);hoist.Pose(true);
        var load=CityBuilder.Box("Crane Load",Origin+new Vector3(28,.7f,4),new Vector3(1.2f,1.2f,1.2f),new Color(.45f,.26f,.12f));load.AddComponent<Rigidbody>().mass=60;load.AddComponent<LiftableLoad>();return m;
    }
    public static ConstructionGrain Grain(string name,Vector3 p,PrimitiveType shape,Vector3 size,Color color,string kind)
    {
        var g=GameObject.CreatePrimitive(shape);g.name=name;g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=CityBuilder.Material(kind,color);
        var material=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/Materials/Granular.physicsMaterial");
        if(!material){material=new PhysicsMaterial("Granular");material.dynamicFriction=.45f;material.staticFriction=.5f;material.bounciness=.02f;AssetDatabase.CreateAsset(material,"Assets/Materials/Granular.physicsMaterial");}
        g.GetComponent<Collider>().sharedMaterial=material;var rb=g.AddComponent<Rigidbody>();rb.mass=.6f;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;rb.interpolation=RigidbodyInterpolation.Interpolate;rb.maxDepenetrationVelocity=3;
        var grain=g.AddComponent<ConstructionGrain>();grain.materialKind=kind;return grain;
    }
    public static void Capture(string name,Vector3 local,Vector3 target)
    {siteCamera.transform.position=Origin+local;siteCamera.transform.LookAt(Origin+target);CityBuilder.Capture(siteCamera,name);}
    public static void Configure()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Practical06_07.unity");CityBuilder.city=GameObject.Find("Virtual City");
        var site=new GameObject("Construction Site").transform;site.position=Origin;
        Part("SiteFloor",site,new Vector3(0,-.15f,0),new Vector3(72,.3f,42),new Color(.65f,.5f,.3f));
        Part("AccessRoad",site,new Vector3(0,-.05f,23),new Vector3(8,.2f,7),new Color(.3f,.32f,.34f));
        foreach(float x in new[]{-36f,36f})Part("SiteFence",site,new Vector3(x,.65f,0),new Vector3(.2f,1.3f,42),new Color(.9f,.67f,.12f));
        Part("SiteFence",site,new Vector3(0,.65f,-21),new Vector3(72,1.3f,.2f),yellow);
        foreach(float x in new[]{-20f,20f})Part("SiteFence",site,new Vector3(x,.65f,21),new Vector3(32,1.3f,.2f),yellow);
        var machines=new[]{Dozer(),Excavator(),Truck(),Crane()};
        var bucket=machines[1].bucket;
        for(int x=0;x<5;x++)for(int z=0;z<4;z++)for(int y=0;y<2;y++)Grain("Bucket Sand",bucket.transform.TransformPoint(new Vector3((x-2)*.26f,-.42f+y*.24f,.3f+z*.25f)),PrimitiveType.Sphere,Vector3.one*.22f,new Color(.88f,.7f,.35f),"Sand");
        for(int x=0;x<8;x++)for(int z=0;z<5;z++)Grain("Dozer Stone",Origin+new Vector3(-21.3f+x*.36f,.2f,-5.3f+z*.36f),PrimitiveType.Sphere,Vector3.one*.3f,new Color(.48f,.48f,.45f),"Stone");
        for(int x=0;x<4;x++)for(int y=0;y<3;y++)Grain("Brick",Origin+new Vector3(12+x*.55f,.18f+y*.3f,-10),PrimitiveType.Cube,new Vector3(.5f,.25f,.3f),new Color(.7f,.29f,.17f),"Brick");
        for(int x=0;x<4;x++)Part("FoundationBeam",site,new Vector3(12+x*3,.3f,12),new Vector3(.4f,.6f,9),new Color(.58f,.59f,.56f));
        siteCamera=CityBuilder.Camera("Camera Site",Origin+new Vector3(44,35,-47),Origin+new Vector3(0,3,1));
        var detail=CityBuilder.Camera("Camera Machine",Origin+new Vector3(-9,9,-22),Origin+new Vector3(-20,1,-9));
        var rig=UnityEngine.Object.FindFirstObjectByType<CityCameraRig>();rig.cameras=rig.cameras.Concat(new[]{siteCamera,detail}).ToArray();rig.Select(4);
        var controls=new GameObject("Construction Controls").AddComponent<ConstructionControls>();controls.machines=machines;controls.detail=detail;
        UnityEngine.Object.FindFirstObjectByType<CityWalker>().enabled=false;Physics.SyncTransforms();
    }
    [MenuItem("Practicals/Build construction site and evidence")]
    public static void Build()
    {
        try{
            Directory.CreateDirectory(CityBuilder.Images);CityBuilder.checks.Clear();Configure();
            CityBuilder.Check(UnityEngine.Object.FindObjectsByType<ConstructionMachine>(FindObjectsSortMode.None).Length==4,"Four controllable construction machines exist");
            CityBuilder.Check(UnityEngine.Object.FindObjectsByType<ConstructionGrain>(FindObjectsSortMode.None).Length==92,"Forty sand grains forty stones and twelve bricks have Rigidbody and colliders");
            CityBuilder.Check(UnityEngine.Object.FindFirstObjectByType<ExcavatorBucket>().contents.Count==0,"Excavator starts with uncaptured physical grains");
            CityBuilder.Check(GameObject.Find("Dump Truck").GetComponent<ConstructionMachine>().bed.GetComponentsInChildren<Collider>().Length==5,"Truck bed has floor side front and tailgate colliders");
            Capture("01_site_overview",new Vector3(44,35,-47),new Vector3(0,3,1));
            Capture("02_site_and_city",new Vector3(-49,43,-50),new Vector3(0,3,23));
            Capture("03_bulldozer",new Vector3(-11,6,-17),new Vector3(-20,1,-8));
            Capture("04_excavator",new Vector3(-14,8,12),new Vector3(-3,2,4));
            Capture("05_truck",new Vector3(12,7,-7),new Vector3(3.2f,1.8f,1));
            Capture("06_tower_crane",new Vector3(35,15,-10),new Vector3(24,7,4));
            Capture("07_bucket_granules",new Vector3(1,4,12),new Vector3(-3,.7f,7));
            Capture("08_stones_and_bricks",new Vector3(16,3,-13),new Vector3(12.8f,.5f,-10));
            siteCamera.transform.position=Origin+new Vector3(44,35,-47);siteCamera.transform.LookAt(Origin+new Vector3(0,3,1));
            string scene="Assets/Scenes/Practical12_16.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene,true)};AssetDatabase.SaveAssets();File.WriteAllLines(Path.Combine(CityBuilder.Root,"verification.txt"),CityBuilder.checks);Debug.Log("CONSTRUCTION_BUILD_SUCCESS");if(Application.isBatchMode)EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);}
    }
    public static void RefreshBricksEvidence()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Practical12_16.unity");
        siteCamera=GameObject.Find("Camera Site").GetComponent<Camera>();
        Capture("08_stones_and_bricks",new Vector3(16,3,-13),new Vector3(12.8f,.5f,-10));
        siteCamera.transform.position=Origin+new Vector3(44,35,-47);siteCamera.transform.LookAt(Origin+new Vector3(0,3,1));
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();EditorApplication.Exit(0);
    }
}
