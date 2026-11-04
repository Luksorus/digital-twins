using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
public static class LabBuilder
{
    static string Root => Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
    static string Images => Path.Combine(Root,"images");
    static List<string> tests = new List<string>();
    static GameObject cube,sphere,cylinder,capsule;
    static Camera overview;
    static Light sun,point,spot;
    static void Check(bool ok,string name) { tests.Add((ok?"PASS ":"FAIL ")+name); if(!ok) throw new Exception(name); }
    static Material Mat(string name,Color color)
    {
        string path="Assets/Materials/"+name+".mat";
        Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m) {m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}
        m.color=color; m.SetFloat("_Glossiness",0.25f); return m;
    }
    static GameObject Primitive(PrimitiveType type,string name,Vector3 p,Vector3 scale,Color color)
    {
        var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.position=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=Mat(name,color);return o;
    }
    static Camera Cam(string name,Vector3 pos,Vector3 target,int display)
    {
        var c=new GameObject(name).AddComponent<Camera>();c.transform.position=pos;c.transform.LookAt(target);c.fieldOfView=48;c.nearClipPlane=0.1f;c.farClipPlane=100;c.targetDisplay=display;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.055f,.075f,.11f);return c;
    }
    static Light Lamp(string name,LightType type,Vector3 pos,Color col,float intensity)
    {
        var l=new GameObject(name).AddComponent<Light>();l.type=type;l.transform.position=pos;l.color=col;l.intensity=intensity;l.shadows=LightShadows.Soft;l.shadowBias=.025f;l.shadowNormalBias=.15f;return l;
    }
    static void BaseScene(bool lab3)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.11f,.13f,.17f);
        QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.VeryHigh;QualitySettings.shadowDistance=80;QualitySettings.pixelLightCount=8;
        Primitive(PrimitiveType.Plane,"Plane",Vector3.zero,new Vector3(2.4f,1,1.8f),new Color(.34f,.38f,.43f));
        cube=Primitive(PrimitiveType.Cube,"Cube",new Vector3(-5,1.1f,0),new Vector3(2.2f,2.2f,1.2f),new Color(.95f,.39f,.12f));
        sphere=Primitive(PrimitiveType.Sphere,"Sphere",new Vector3(-1.7f,.9f,0),new Vector3(2.3f,1.8f,1.2f),new Color(.16f,.68f,.89f));
        cylinder=Primitive(PrimitiveType.Cylinder,"Cylinder",new Vector3(1.7f,1.3f,0),new Vector3(1.4f,1.3f,.9f),new Color(.35f,.79f,.37f));
        capsule=Primitive(PrimitiveType.Capsule,"Capsule",new Vector3(5,1.5f,0),new Vector3(1.2f,1.5f,.85f),new Color(.7f,.35f,.86f));
        // Markers are real scene objects and help compare the camera views.
        for(int i=0;i<4;i++) {var g=new GameObject("Label_"+i);g.transform.position=new Vector3(-5+i*3.33f,.025f,-1.4f);g.transform.rotation=Quaternion.Euler(90,0,0);var t=g.AddComponent<TextMesh>();t.text=new[]{"CUBE","ELLIPSOID","CYLINDER","CAPSULE"}[i];t.characterSize=.12f;t.fontSize=48;t.anchor=TextAnchor.MiddleCenter;t.color=Color.white;}
        sun=Lamp("Directional Light",LightType.Directional,new Vector3(0,7,0),new Color(1,.91f,.78f),.85f);sun.transform.rotation=Quaternion.Euler(48,-32,0);
        point=Lamp("Point Light",LightType.Point,new Vector3(-3,3,-2),new Color(.2f,.65f,1),5);point.range=8;
        spot=Lamp("Spot Light",LightType.Spot,new Vector3(4,6,-3),new Color(1,.55f,.2f),9);spot.range=14;spot.spotAngle=55;spot.innerSpotAngle=28;spot.transform.LookAt(new Vector3(3,0,0));
        overview=Cam("Camera Directional Display 1",new Vector3(0,11,-17),new Vector3(0,.3f,0),0);
        Cam("Camera Point Display 2",new Vector3(-10,7,-12),new Vector3(-2,.6f,0),1);
        Cam("Camera Spot Display 3",new Vector3(10,8,-12),new Vector3(3,.7f,0),2);
        overview.gameObject.AddComponent<DisplayActivator>();
        SceneView.lastActiveSceneView?.LookAt(new Vector3(0,0,0),Quaternion.Euler(35,0,0),18);
    }
    static void Capture(Camera camera,string name)
    {
        var rt=new RenderTexture(1600,1000,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Images,name+".png"),tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
    }
    static void Isolate(bool d,bool p,bool s,string name,Camera cam=null) {sun.enabled=d;point.enabled=p;spot.enabled=s;Capture(cam?cam:overview,name);sun.enabled=point.enabled=spot.enabled=true;}
    [MenuItem("Practicals/Build scenes and evidence")]
    public static void Build()
    {
        try {
        tests.Clear();
        Directory.CreateDirectory(Images);Directory.CreateDirectory("Assets/Scenes");Directory.CreateDirectory("Assets/Materials");
        BaseScene(false);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),"Assets/Scenes/Practical02.unity");
        Capture(overview,"02_01_overview");
        foreach(var c in UnityEngine.Object.FindObjectsByType<Camera>())Capture(c,"02_camera_"+(c.targetDisplay+1));
        Isolate(true,false,false,"02_02_directional");Isolate(false,true,false,"02_03_point");Isolate(false,false,true,"02_04_spot");
        var rot=sun.transform.rotation;sun.transform.rotation=Quaternion.Euler(23,-32,0);Isolate(true,false,false,"02_05_long_shadows");sun.transform.rotation=rot;
        var top=Cam("Temporary Top",new Vector3(0,17,0),Vector3.zero,0);top.orthographic=true;top.orthographicSize=8;Capture(top,"02_06_top");UnityEngine.Object.DestroyImmediate(top.gameObject);
        Check(UnityEngine.Object.FindObjectsByType<Camera>().Length==3,"Lab2 three cameras");Check(cube.transform.localScale.x!=cube.transform.localScale.z,"Lab2 cuboid");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Practical02.unity",true)};
        AssetDatabase.SaveAssets();File.WriteAllLines(Path.Combine(Root,"verification.txt"),tests);Debug.Log("LAB_BUILD_SUCCESS");
        if(Application.isBatchMode)EditorApplication.Exit(0);
        }catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);}
    }
}
