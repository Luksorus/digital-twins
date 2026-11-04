using UnityEngine;
using System.Collections.Generic;

public class CitySimulation : MonoBehaviour
{
    public TrafficRoute route;
    public TrafficCar carPrefab;
    public int carCount=8;
    public List<TrafficCar> cars=new List<TrafficCar>();
    public PedestrianMover[] pedestrians;
    public StationTrain train;
    public Renderer carSignal,pedestrianSignal;
    public float elapsed;
    public bool paused;
    public bool CarsGreen
    {
        get {
            bool scheduled=Mathf.Repeat(elapsed,24f)<10f||Mathf.Repeat(elapsed,24f)>=20f;
            if(!scheduled)return false;
            if(pedestrians!=null)foreach(var p in pedestrians)
                if(p.crossing&&p.progress>.001f&&p.progress<Vector3.Distance(p.start,p.end)-.001f)return false;
            return true;
        }
    }
    public bool PedestriansGreen => Mathf.Repeat(elapsed,24f)>=12f&&Mathf.Repeat(elapsed,24f)<18f;
    public void Initialize()
    {
        if(cars.Count>0)return;
        for(int i=0;i<carCount;i++){
            var car=Instantiate(carPrefab,transform);car.name="TrafficCar_"+i;car.route=route;car.Place(i*route.Length/carCount);cars.Add(car);
        }
    }
    void Start(){Initialize();}
    void Update(){if(Input.GetKeyDown(KeyCode.Space))paused=!paused;}
    void FixedUpdate(){if(!paused)Step(Time.fixedDeltaTime,false);}
    public void Step(float dt,bool manual)
    {
        elapsed+=dt;
        var targets=new float[cars.Count];
        for(int i=0;i<cars.Count;i++){
            var c=cars[i];float target=route.SpeedLimit(c.distance);
            float nearest=route.Length;
            for(int j=0;j<cars.Count;j++)if(i!=j)nearest=Mathf.Min(nearest,Mathf.Repeat(cars[j].distance-c.distance,route.Length));
            target=Mathf.Min(target,Mathf.Sqrt(2*c.braking*Mathf.Max(0,nearest-7f)));
            float stopDistance=Mathf.Repeat(20f-c.distance,route.Length);
            if(!CarsGreen&&stopDistance<30f)target=Mathf.Min(target,Mathf.Sqrt(2*c.braking*Mathf.Max(0,stopDistance-.3f)));
            targets[i]=target;
        }
        for(int i=0;i<cars.Count;i++)cars[i].Step(dt,targets[i],manual);
        foreach(var pedestrian in pedestrians)pedestrian.Step(dt,PedestriansGreen);
        train.Step(dt);
        if(carSignal)carSignal.sharedMaterial.color=CarsGreen?Color.green:Color.red;
        if(pedestrianSignal)pedestrianSignal.sharedMaterial.color=PedestriansGreen?Color.green:Color.red;
    }
    void OnGUI()
    {
        GUI.Box(new Rect(16,94,620,62),"CITY SIMULATION  |  Space pause / resume");
        GUI.Label(new Rect(28,118,590,36),"Cars "+cars.Count+"  Pedestrians "+pedestrians.Length+"  Train "+train.phase+"  "+train.speed.ToString("F1")+" m/s\nTraffic: "+(CarsGreen?"GREEN":"RED")+"  Crossing: "+(PedestriansGreen?"WALK":"WAIT"));
    }
}
