using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class CityBuilder
{
    public static string Root => Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
    public static string Images => Path.Combine(Root,"images");
    public static List<string> checks = new List<string>();
    public static List<GameObject> buildings = new List<GameObject>();
    public static Camera overview, street, station, map;
    public static Light sun;
    public static GameObject city;
    public static void Check(bool success,string name)
    {
        checks.Add((success?"PASS ":"FAIL ")+name);
        if (!success) throw new Exception(name);
    }
    public static Material Material(string name,Color color)
    {
        string path="Assets/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){ m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path); }
        m.color=color;m.SetFloat("_Glossiness",.12f);return m;
    }
    public static GameObject Box(string name,Vector3 p,Vector3 size,Color color,Transform parent=null,bool collider=true)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.position=p;g.transform.localScale=size;
        if(parent)g.transform.SetParent(parent,true);
        g.GetComponent<Renderer>().sharedMaterial=Material(name.Split('_')[0],color);
        if(!collider) UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }
    public static Bounds Bounds(GameObject g)
    {
        var renderers=g.GetComponentsInChildren<Renderer>();
        var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);return b;
    }
    public static GameObject Asset(string kit,string model,Vector3 position,float width,float yaw,string name,bool collider)
    {
        string path="Assets/ThirdParty/Kenney/"+kit+"/Models/"+model+".fbx";
        var original=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if(!original)throw new Exception("Missing asset "+path);
        var root=new GameObject(name);root.transform.SetParent(city.transform);
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(original);visual.transform.SetParent(root.transform,false);
        var b=Bounds(visual);float scale=width/Mathf.Max(b.size.x,b.size.z);visual.transform.localScale*=scale;
        visual.transform.localRotation=Quaternion.Euler(0,yaw,0);
        b=Bounds(visual);visual.transform.position-=new Vector3(b.center.x,b.min.y,b.center.z);
        root.transform.position=position;
        // Use the pack's palette texture explicitly with the Built-in Standard shader.
        var palette=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/Kenney/"+kit+"/Models/Textures/colormap.png");
        string matPath="Assets/Materials/Kenney_"+kit+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if(!mat){mat=new Material(Shader.Find("Standard"));mat.mainTexture=palette;mat.SetFloat("_Glossiness",.15f);AssetDatabase.CreateAsset(mat,matPath);}
        foreach(var r in visual.GetComponentsInChildren<Renderer>()) r.sharedMaterials=Enumerable.Repeat(mat,r.sharedMaterials.Length).ToArray();
        if(collider){b=Bounds(root);var c=root.AddComponent<BoxCollider>();c.center=root.transform.InverseTransformPoint(b.center);c.size=b.size;root.isStatic=true;}
        return root;
    }
    public static Camera Camera(string name,Vector3 pos,Vector3 target,bool ortho=false,float size=50)
    {
        var c=new GameObject(name).AddComponent<Camera>();c.transform.position=pos;c.transform.LookAt(target);
        c.fieldOfView=52;c.nearClipPlane=.1f;c.farClipPlane=350;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.63f,.79f,.89f);
        c.orthographic=ortho;c.orthographicSize=size;return c;
    }
    public static void Text(string text,Vector3 p,float scale,Quaternion rot)
    {
        var g=new GameObject("Sign_"+text);g.transform.SetParent(city.transform);g.transform.position=p;g.transform.rotation=rot;
        var t=g.AddComponent<TextMesh>();t.text=text;t.fontSize=64;t.characterSize=scale;t.color=new Color(.93f,.96f,1);t.anchor=TextAnchor.MiddleCenter;
    }
    public static void Setup()
    {
        Directory.CreateDirectory(Images);Directory.CreateDirectory("Assets/Scenes");Directory.CreateDirectory("Assets/Materials");Directory.CreateDirectory("Assets/Prefabs");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);buildings.Clear();city=new GameObject("Virtual City");
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.64f,.69f,.75f);
        QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.VeryHigh;QualitySettings.shadowDistance=250;QualitySettings.pixelLightCount=8;
        sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(48,-32,0);sun.intensity=1.15f;sun.shadows=LightShadows.Soft;
        Box("Ground",new Vector3(0,-.25f,2),new Vector3(118,.5f,106),new Color(.33f,.51f,.33f),city.transform);
        Color road=new Color(.18f,.22f,.25f),walk=new Color(.66f,.7f,.7f),line=new Color(.95f,.92f,.76f);
        foreach(float z in new[]{-24f,0f,24f}){
            Box("Sidewalk",new Vector3(0,.035f,z),new Vector3(78,.1f,13),walk,city.transform);
            Box("Road",new Vector3(0,.105f,z),new Vector3(78,.045f,8),road,city.transform);
            for(int x=-36;x<=36;x+=5)if(Mathf.Abs(x)>5&&Mathf.Abs(Mathf.Abs(x)-32)>5)Box("LaneMark",new Vector3(x,.132f,z),new Vector3(2,.012f,.12f),line,city.transform,false);
        }
        foreach(float x in new[]{-32f,0f,32f}){
            Box("Sidewalk",new Vector3(x,.037f,0),new Vector3(13,.1f,60),walk,city.transform);
            Box("Road",new Vector3(x,.108f,0),new Vector3(8,.045f,60),road,city.transform);
            for(int z=-26;z<=26;z+=5)if(Mathf.Abs(z)>5&&Mathf.Abs(Mathf.Abs(z)-24)>5)Box("LaneMark",new Vector3(x,.135f,z),new Vector3(.12f,.012f,2),line,city.transform,false);
        }
        foreach(float x in new[]{-32f,0f,32f})foreach(float z in new[]{-24f,0f,24f})for(int i=0;i<7;i++){
            Box("Crosswalk",new Vector3(x-3+i,.14f,z-5),new Vector3(.55f,.012f,2),Color.white,city.transform,false);
            Box("Crosswalk",new Vector3(x+5,.142f,z-3+i),new Vector3(2,.012f,.55f),Color.white,city.transform,false);
        }
        int index=0;
        foreach(float x in new[]{-23f,-11f,11f,23f})foreach(float z in new[]{-15f,12f}){
            string model="building-type-"+((char)('a'+index%12));
            var b=Asset("city-kit-suburban",model,new Vector3(x,.13f,z),8f,z<0?0:180,"House_"+index,true);buildings.Add(b);index++;
        }
        foreach(var info in new[]{new Vector3(-46,.1f,9),new Vector3(-46,.1f,-11),new Vector3(46,.1f,-9),new Vector3(46,.1f,12)}){
            string model="building-skyscraper-"+((char)('a'+index%5));var b=Asset("city-kit-commercial",model,info,10f,0,"Office_"+index,true);buildings.Add(b);index++;
        }
        // Park is separated from the vehicle lanes by wide sidewalks.
        Box("Park",new Vector3(15,.15f,-.8f),new Vector3(18,.07f,8),new Color(.25f,.57f,.34f),city.transform);
        for(int i=0;i<14;i++){
            Vector3 p=i<6?new Vector3(-26+i*10,.15f,-33):new Vector3(-27+(i-6)*8,.15f,33);
            Asset("city-kit-suburban",i%2==0?"tree-large":"tree-small",p,3.4f,0,"Tree_"+i,true);
        }
        foreach(float x in new[]{10f,16f,22f})Asset("city-kit-suburban","tree-large",new Vector3(x,.2f,-.8f),2.6f,0,"ParkTree_"+x,true);
        for(int i=0;i<4;i++){
            Vector3 p=new Vector3(-22+i*13,.16f,29.4f);
            Asset("car-kit",new[]{"sedan","taxi","suv","delivery"}[i],p,4.2f,90,"ParkedCar_"+i,true);
        }
        Box("StationPlatform",new Vector3(6,.55f,35.5f),new Vector3(39,1.1f,5),walk,city.transform);
        Box("StationCanopy",new Vector3(6,4.4f,35.5f),new Vector3(35,.35f,5.5f),new Color(.12f,.32f,.43f),city.transform);
        foreach(float x in new[]{-9f,1f,11f,21f})Box("StationPost",new Vector3(x,2.5f,35.5f),new Vector3(.2f,4,.2f),road,city.transform);
        Box("TrackBallast",new Vector3(0,.1f,40),new Vector3(150,.15f,4),new Color(.37f,.38f,.38f),city.transform);
        for(int x=-73;x<=73;x+=2)Box("Sleeper",new Vector3(x,.23f,40),new Vector3(.3f,.14f,3),new Color(.27f,.22f,.17f),city.transform,false);
        foreach(float z in new[]{39f,41f})Box("Rail",new Vector3(0,.37f,z),new Vector3(150,.17f,.1f),new Color(.7f,.74f,.77f),city.transform,false);
        Text("CENTRAL STATION",new Vector3(6,3.8f,32.65f),.22f,Quaternion.identity);
        Text("CITY LAB  |  KENNEY CC0",new Vector3(0,.17f,-39),.35f,Quaternion.Euler(90,0,0));
        overview=Camera("Camera City",new Vector3(86,77,-96),new Vector3(0,2,4));
        station=Camera("Camera Station",new Vector3(51,28,64),new Vector3(5,1.5f,31));
        map=Camera("Camera Map",new Vector3(0,100,0),Vector3.zero,true,58);
        var walker=new GameObject("Walker");walker.transform.position=new Vector3(-23,.15f,-29.2f);walker.transform.rotation=Quaternion.identity;
        var controller=walker.AddComponent<CharacterController>();controller.height=1.8f;controller.radius=.32f;controller.center=Vector3.up*.9f;controller.skinWidth=.025f;walker.AddComponent<CityWalker>();
        street=Camera("Camera Walk",walker.transform.position+Vector3.up*1.65f,walker.transform.position+new Vector3(0,1.65f,15));street.transform.SetParent(walker.transform,true);
        var rig=new GameObject("Camera Controls").AddComponent<CityCameraRig>();rig.cameras=new[]{overview,street,station,map};rig.Select(0);
    }
    public static void Capture(Camera c,string name)
    {
        var rt=new RenderTexture(1700,1100,24);var old=c.targetTexture;bool enabled=c.enabled;c.enabled=true;c.targetTexture=rt;
        c.Render();RenderTexture.active=rt;var t=new Texture2D(1700,1100,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1700,1100),0,0);t.Apply();
        File.WriteAllBytes(Path.Combine(Images,name+".png"),t.EncodeToPNG());RenderTexture.active=null;c.targetTexture=old;c.enabled=enabled;UnityEngine.Object.DestroyImmediate(t);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
    }
    static void CollisionTest()
    {
        var target=buildings[0].GetComponent<BoxCollider>();Physics.SyncTransforms();Bounds b=target.bounds;
        var g=new GameObject("Collision Probe");g.transform.position=new Vector3(b.center.x,.15f,b.min.z-3);
        var c=g.AddComponent<CharacterController>();c.height=1.8f;c.radius=.32f;c.center=Vector3.up*.9f;c.skinWidth=.025f;
        var p=g.AddComponent<CityWalker>();CollisionFlags flags=CollisionFlags.None;
        for(int i=0;i<120;i++)flags|=p.Step(Vector3.forward,1f/60);
        Check((flags&CollisionFlags.Sides)!=0,"CharacterController reports wall collision");
        Check(g.transform.position.z<b.min.z,"Walker cannot pass through building wall");
        File.WriteAllText(Path.Combine(Root,"collision.json"),JsonUtility.ToJson(new CollisionEvidence{wallZ=b.min.z,stoppedZ=g.transform.position.z,radius=c.radius},true));
        var cam=Camera("Collision detail",new Vector3(b.center.x-11,9,b.min.z-13),b.center);
        var marker=GameObject.CreatePrimitive(PrimitiveType.Capsule);marker.transform.position=g.transform.position+Vector3.up*.9f;marker.transform.localScale=new Vector3(.64f,.9f,.64f);marker.GetComponent<Renderer>().sharedMaterial=Material("Probe",Color.yellow);
        Capture(cam,"10_wall_collision");UnityEngine.Object.DestroyImmediate(cam.gameObject);UnityEngine.Object.DestroyImmediate(marker);UnityEngine.Object.DestroyImmediate(g);
    }
    [Serializable]public class CollisionEvidence{public float wallZ,stoppedZ,radius;}
    [MenuItem("Practicals/Build city and evidence")]
    public static void Build()
    {
        try{
            checks.Clear();Setup();
            string scene="Assets/Scenes/Practical04_05.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),scene);
            Capture(overview,"01_city_overview");Capture(map,"02_city_map");Capture(street,"03_walk_view");Capture(station,"04_station");
            var detail=Camera("Detail",new Vector3(-42,16,-32),new Vector3(-19,2,-10));Capture(detail,"05_residential");
            detail.transform.position=new Vector3(65,34,-38);detail.transform.LookAt(new Vector3(40,8,0));Capture(detail,"06_business");
            detail.transform.position=new Vector3(25,11,-11);detail.transform.LookAt(new Vector3(16,1,0));Capture(detail,"07_park");
            detail.transform.position=new Vector3(-8,15,-34);detail.transform.LookAt(new Vector3(0,0,-24));Capture(detail,"08_crosswalk");
            detail.transform.position=new Vector3(-38,12,41);detail.transform.LookAt(new Vector3(-5,1,29));Capture(detail,"09_parking");UnityEngine.Object.DestroyImmediate(detail.gameObject);
            Check(buildings.Count==12,"Twelve imported Kenney buildings placed");
            Check(buildings.All(b=>b.GetComponent<BoxCollider>()!=null),"Every building has a BoxCollider");
            Check(buildings.All(b=>b.isStatic),"Buildings are static geometry");
            Check(AssetDatabase.FindAssets("t:Model",new[]{"Assets/ThirdParty/Kenney"}).Length>=30,"Free FBX asset packs imported");
            Check(UnityEngine.Object.FindObjectsByType<Camera>().Length==4,"Four inspection cameras exist");
            Check(buildings.All(b=>Bounds(b).size.y>1f),"Imported building meshes have nonzero dimensions");
            CollisionTest();
            Check(!Physics.Raycast(new Vector3(-23,2,-29),Vector3.forward,1),"Walkable sidewalk is initially clear");
            var first=buildings[0];var prefab=PrefabUtility.SaveAsPrefabAsset(first,"Assets/Prefabs/ResidentialBuilding.prefab");Check(prefab!=null,"Building prefab saved with collider");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene,true)};AssetDatabase.SaveAssets();
            File.WriteAllLines(Path.Combine(Root,"verification.txt"),checks);
            Debug.Log("CITY_BUILD_SUCCESS");if(Application.isBatchMode)EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);File.WriteAllLines(Path.Combine(Root,"verification.txt"),checks);if(Application.isBatchMode)EditorApplication.Exit(1);}
    }
}
