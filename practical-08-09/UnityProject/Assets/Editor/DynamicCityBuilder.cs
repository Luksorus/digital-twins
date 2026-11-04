using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class DynamicCityBuilder
{
    static CitySimulation simulation;
    static string Root=>CityBuilder.Root;
    static List<string> rows=new List<string>();
    static void Check(bool ok,string text){CityBuilder.Check(ok,text);}
    static GameObject Part(string name,Transform root,Vector3 local,Vector3 size,Color color)
    {
        var p=CityBuilder.Box(name,local,size,color,null,false);p.transform.SetParent(root,false);return p;
    }
    static PedestrianMover Person(string name,Vector3 start,Vector3 end,float progress,bool crossing)
    {
        var g=new GameObject(name);var p=g.AddComponent<PedestrianMover>();p.start=start;p.end=end;p.progress=progress;p.crossing=crossing;
        Color coat=new Color(.18f+(progress%4)*.16f,.27f,.63f);
        Part("PedBody",g.transform,new Vector3(0,1.12f,0),new Vector3(.42f,.65f,.25f),coat);
        var head=GameObject.CreatePrimitive(PrimitiveType.Sphere);head.name="Head";UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());head.transform.SetParent(g.transform,false);head.transform.localPosition=new Vector3(0,1.67f,0);head.transform.localScale=Vector3.one*.34f;head.GetComponent<Renderer>().sharedMaterial=CityBuilder.Material("Skin",new Color(.85f,.63f,.43f));
        Transform Limb(string label,Vector3 hip,Vector3 offset,Vector3 scale,Color c){var pivot=new GameObject(label).transform;pivot.SetParent(g.transform,false);pivot.localPosition=hip;Part(label+"Mesh",pivot,offset,scale,c);return pivot;}
        p.leftLeg=Limb("LeftLeg",new Vector3(-.12f,.8f,0),new Vector3(0,-.37f,0),new Vector3(.16f,.74f,.19f),new Color(.16f,.19f,.25f));
        p.rightLeg=Limb("RightLeg",new Vector3(.12f,.8f,0),new Vector3(0,-.37f,0),new Vector3(.16f,.74f,.19f),new Color(.16f,.19f,.25f));
        p.leftArm=Limb("LeftArm",new Vector3(-.3f,1.4f,0),new Vector3(0,-.27f,0),new Vector3(.14f,.55f,.16f),coat);
        p.rightArm=Limb("RightArm",new Vector3(.3f,1.4f,0),new Vector3(0,-.27f,0),new Vector3(.14f,.55f,.16f),coat);
        var capsule=g.AddComponent<CapsuleCollider>();capsule.radius=.3f;capsule.height=1.85f;capsule.center=Vector3.up*.925f;
        p.Step(0,false);return p;
    }
    static StationTrain Train()
    {
        var root=new GameObject("Passenger Train");var rb=root.AddComponent<Rigidbody>();rb.isKinematic=true;rb.useGravity=false;rb.interpolation=RigidbodyInterpolation.Interpolate;var train=root.AddComponent<StationTrain>();var wheels=new List<Transform>();
        for(int car=0;car<4;car++){
            var coach=new GameObject(car==0?"Motor Coach":"Passenger Coach "+car).transform;coach.SetParent(root.transform,false);coach.localPosition=new Vector3(-car*8.6f,0,0);
            Color blue=new Color(.06f,.29f,.48f);
            Part("TrainBody",coach,new Vector3(0,1.5f,0),new Vector3(8,2.2f,2.5f),blue);
            Part("TrainRoof",coach,new Vector3(0,2.68f,0),new Vector3(7.7f,.23f,2.35f),new Color(.8f,.84f,.87f));
            Part("TrainStripe",coach,new Vector3(0,.83f,0),new Vector3(8,.22f,2.56f),new Color(1,.64f,.13f));
            for(int side=-1;side<=1;side+=2)for(int window=0;window<6;window++)Part("TrainWindow",coach,new Vector3(-3.1f+window*1.24f,1.9f,side*1.265f),new Vector3(.87f,.7f,.035f),new Color(.62f,.84f,.92f));
            if(car==0)Part("TrainFrontWindow",coach,new Vector3(4.02f,1.93f,0),new Vector3(.035f,.84f,1.9f),new Color(.62f,.84f,.92f));
            foreach(float x in new[]{-2.7f,2.7f})foreach(float z in new[]{-1.17f,1.17f}){
                var wheel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);wheel.name="Wheel";UnityEngine.Object.DestroyImmediate(wheel.GetComponent<Collider>());wheel.transform.SetParent(coach,false);wheel.transform.localPosition=new Vector3(x,.47f,z);wheel.transform.localRotation=Quaternion.Euler(90,0,0);wheel.transform.localScale=new Vector3(.9f,.15f,.9f);wheel.GetComponent<Renderer>().sharedMaterial=CityBuilder.Material("TrainWheel",new Color(.12f,.14f,.17f));wheels.Add(wheel.transform);
            }
            var collider=coach.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,1.55f,0);collider.size=new Vector3(8,2.5f,2.55f);
        }
        train.wheels=wheels.ToArray();train.ResetTrip();return train;
    }
    static void Configure()
    {
        CityBuilder.Setup();
        var route=new GameObject("Traffic Route").AddComponent<TrafficRoute>();route.Build();
        var car=CityBuilder.Asset("car-kit","sedan",Vector3.zero,4.2f,0,"Car Template",true);car.isStatic=false;
        var body=car.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
        var component=car.AddComponent<TrafficCar>();component.route=route;
        var prefab=PrefabUtility.SaveAsPrefabAsset(car,"Assets/Prefabs/TrafficCar.prefab").GetComponent<TrafficCar>();UnityEngine.Object.DestroyImmediate(car);
        var pedestrians=new List<PedestrianMover>();
        for(int i=0;i<6;i++)pedestrians.Add(Person("Sidewalk Pedestrian "+i,new Vector3(-27,.16f,-29.4f),new Vector3(27,.16f,-29.4f),i*8.5f,false));
        pedestrians.Add(Person("Crossing Pedestrian A",new Vector3(5,.16f,-30),new Vector3(5,.16f,-18),0,true));
        pedestrians.Add(Person("Crossing Pedestrian B",new Vector3(5.7f,.16f,-30),new Vector3(5.7f,.16f,-18),12,true));
        foreach(var p in pedestrians){var saved=PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/Prefabs/Pedestrian.prefab");if(saved)break;}
        var train=Train();var trainPrefab=PrefabUtility.SaveAsPrefabAsset(train.gameObject,"Assets/Prefabs/PassengerTrain.prefab");
        UnityEngine.Object.DestroyImmediate(train.gameObject);train=((GameObject)PrefabUtility.InstantiatePrefab(trainPrefab)).GetComponent<StationTrain>();
        simulation=new GameObject("City Simulation").AddComponent<CitySimulation>();simulation.route=route;simulation.carPrefab=prefab;simulation.pedestrians=pedestrians.ToArray();simulation.train=train;simulation.Initialize();
        foreach(var p in pedestrians)p.transform.SetParent(simulation.transform,true);
        var pole=CityBuilder.Box("SignalPole",new Vector3(-3,.9f,-30.3f),new Vector3(.14f,1.8f,.14f),new Color(.18f,.2f,.23f));
        simulation.carSignal=CityBuilder.Box("CarSignal",new Vector3(-3,2f,-30.3f),new Vector3(.5f,.6f,.25f),Color.green).GetComponent<Renderer>();
        simulation.pedestrianSignal=CityBuilder.Box("PedestrianSignal",new Vector3(2,1.8f,-30.3f),new Vector3(.35f,.5f,.2f),Color.red).GetComponent<Renderer>();
        CityBuilder.station.transform.position=new Vector3(39,19,59);CityBuilder.station.transform.LookAt(new Vector3(0,1,38));
    }
    static void TestAndCapture()
    {
        Check(simulation.cars.Count==8,"Eight cars instantiated from TrafficCar prefab");
        Check(simulation.cars.All(c=>c.GetComponent<Rigidbody>().isKinematic),"Cars have interpolated kinematic Rigidbody");
        Check(simulation.pedestrians.Length==8,"Eight animated pedestrians exist");
        Check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PassengerTrain.prefab")!=null,"Passenger train prefab created");
        float initial=simulation.cars[0].distance;Vector3 pedStart=simulation.pedestrians[0].transform.position;
        CityBuilder.Capture(CityBuilder.overview,"01_dynamic_initial");
        bool approach=false,brake=false,dwell=false,depart=false,red=false,walk=false;
        float maxAccel=0,minGap=float.MaxValue;float stoppedAt=0;int redStops=0;bool penetration=false,pedestrianPenetration=false;
        float previousTrainSpeed=0,previousCarSpeed=0;int previousCycles=0;
        rows.Clear();rows.Add("time,train_x,train_speed,train_phase,car0_distance,car0_speed,cars_green,pedestrians_green");
        for(int frame=0;frame<3000;frame++){
            simulation.Step(.02f,true);Physics.SyncTransforms();float time=simulation.elapsed;
            if(simulation.train.cycles==previousCycles)maxAccel=Mathf.Max(maxAccel,Mathf.Abs(simulation.train.speed-previousTrainSpeed)/.02f);
            previousCycles=simulation.train.cycles;previousTrainSpeed=simulation.train.speed;
            if(Mathf.Abs(simulation.cars[0].speed-previousCarSpeed)>simulation.cars[0].braking*.02f+.001f)throw new Exception("Car acceleration discontinuity");previousCarSpeed=simulation.cars[0].speed;
            for(int i=0;i<simulation.cars.Count;i++)for(int j=i+1;j<simulation.cars.Count;j++){
                float gap=Mathf.Abs(simulation.cars[i].distance-simulation.cars[j].distance);gap=Mathf.Min(gap,simulation.route.Length-gap);minGap=Mathf.Min(minGap,gap);
                if(frame%50==0){var a=simulation.cars[i].GetComponent<BoxCollider>();var b=simulation.cars[j].GetComponent<BoxCollider>();if(Physics.ComputePenetration(a,a.transform.position,a.transform.rotation,b,b.transform.position,b.transform.rotation,out _,out _))penetration=true;}
            }
            if(frame%25==0)foreach(var car in simulation.cars)foreach(var p in simulation.pedestrians){var a=car.GetComponent<BoxCollider>();var b=p.GetComponent<CapsuleCollider>();if(Physics.ComputePenetration(a,a.transform.position,a.transform.rotation,b,b.transform.position,b.transform.rotation,out _,out _))pedestrianPenetration=true;}
            if(!simulation.CarsGreen&&simulation.cars.Any(c=>c.speed<.1f&&Mathf.Abs(c.distance-20)<1.5f))redStops++;
            if(frame%50==0)rows.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0:F2},{1:F3},{2:F3},{3},{4:F3},{5:F3},{6},{7}",time,simulation.train.positionX,simulation.train.speed,simulation.train.phase,simulation.cars[0].distance,simulation.cars[0].speed,simulation.CarsGreen,simulation.PedestriansGreen));
            if(!approach&&simulation.train.positionX>-27&&simulation.train.phase!=StationTrain.Phase.Dwell){CityBuilder.Capture(CityBuilder.station,"02_train_approach");approach=true;}
            if(!brake&&simulation.train.phase==StationTrain.Phase.Braking&&simulation.train.positionX>3){CityBuilder.Capture(CityBuilder.station,"03_train_braking");brake=true;}
            if(!dwell&&simulation.train.phase==StationTrain.Phase.Dwell){CityBuilder.Capture(CityBuilder.station,"04_train_dwell");stoppedAt=time;dwell=true;}
            if(dwell&&simulation.train.phase==StationTrain.Phase.Dwell){if(Mathf.Abs(simulation.train.positionX-simulation.train.stopX)>.001f||simulation.train.speed!=0)throw new Exception("Train creeps during dwell");}
            if(!depart&&simulation.train.phase==StationTrain.Phase.Depart&&simulation.train.speed>4){Check(time-stoppedAt>=5.99f,"Train dwell lasts at least six seconds");CityBuilder.Capture(CityBuilder.station,"05_train_departure");depart=true;}
            if(!red&&simulation.elapsed>14){var cam=CityBuilder.Camera("Crossing red",new Vector3(-15,10,-39),new Vector3(-1,1,-25));CityBuilder.Capture(cam,"06_red_and_crossing");UnityEngine.Object.DestroyImmediate(cam.gameObject);red=true;}
            if(!walk&&simulation.elapsed>18){var cam=CityBuilder.Camera("Pedestrians",new Vector3(10,5,-37),new Vector3(0,1,-24));CityBuilder.Capture(cam,"07_pedestrians");UnityEngine.Object.DestroyImmediate(cam.gameObject);walk=true;}
            if(frame==400){CityBuilder.Capture(CityBuilder.overview,"08_cars_moving");CityBuilder.Capture(CityBuilder.map,"09_traffic_map");}
            if(frame==1900){var cam=CityBuilder.Camera("Traffic close",new Vector3(40,6,-34),new Vector3(24,1,-24));CityBuilder.Capture(cam,"10_traffic_corner");UnityEngine.Object.DestroyImmediate(cam.gameObject);}
        }
        Check(initial!=simulation.cars[0].distance,"Cars advance along the route");
        Check(Vector3.Distance(pedStart,simulation.pedestrians[0].transform.position)>1,"Pedestrians move on sidewalks");
        Check(minGap>5,"Following distance stays above five meters");Check(!penetration,"Vehicle colliders do not overlap in sampled physics checks");
        Check(!pedestrianPenetration,"Cars and pedestrian capsules do not overlap in sampled physics checks");
        Check(redStops>0,"Cars stop before red pedestrian crossing");
        Check(approach&&brake&&dwell&&depart,"Train approaches brakes stops and departs");Check(maxAccel<2f,"Train acceleration stays below two meters per second squared");
        Check(simulation.train.cycles>=1,"Train completes and restarts a full off-screen cycle");
        Check(simulation.train.stops>=1&&simulation.train.departures>=1,"Station events recorded");
        File.WriteAllLines(Path.Combine(Root,"telemetry.csv"),rows);
        File.WriteAllText(Path.Combine(Root,"metrics.json"),"{\"minimum_car_gap\":"+minGap.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"max_train_acceleration\":"+maxAccel.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"red_stop_steps\":"+redStops+"}");
    }
    [MenuItem("Practicals/Build dynamic city and evidence")]
    public static void Build()
    {
        try{
            CityBuilder.checks.Clear();Configure();TestAndCapture();
            Configure();foreach(var car in simulation.cars)UnityEngine.Object.DestroyImmediate(car.gameObject);simulation.cars.Clear();
            string path="Assets/Scenes/Practical06_07.unity";EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),path);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(path,true)};AssetDatabase.SaveAssets();
            File.WriteAllLines(Path.Combine(Root,"verification.txt"),CityBuilder.checks);Debug.Log("DYNAMIC_CITY_SUCCESS");
            if(Application.isBatchMode)EditorApplication.Exit(0);
        }catch(Exception e){Debug.LogException(e);File.WriteAllLines(Path.Combine(Root,"verification.txt"),CityBuilder.checks);if(Application.isBatchMode)EditorApplication.Exit(1);}
    }
}
