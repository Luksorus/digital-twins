using UnityEngine;

public class ConstructionControls : MonoBehaviour
{
    public ConstructionMachine[] machines;
    public int selected;
    public Camera detail;
    public bool automaticTest;
    void Update()
    {
        if(automaticTest)return;
        for(int i=0;i<4;i++)if(Input.GetKeyDown(KeyCode.Alpha1+i))selected=i;
        foreach(var machine in machines){machine.drive=0;machine.steer=0;}
        var m=machines[selected];float dt=Time.deltaTime;
        m.drive=Axis(KeyCode.UpArrow,KeyCode.DownArrow);m.steer=Axis(KeyCode.RightArrow,KeyCode.LeftArrow);
        float q=Axis(KeyCode.E,KeyCode.Q),r=Axis(KeyCode.R,KeyCode.F),t=Axis(KeyCode.T,KeyCode.G),y=Axis(KeyCode.Y,KeyCode.H);
        if(m.kind==ConstructionMachine.Kind.Excavator){m.yaw+=q*35*dt;m.boomAngle=Mathf.Clamp(m.boomAngle-r*25*dt,-65,0);m.stickAngle=Mathf.Clamp(m.stickAngle+t*30*dt,10,100);m.bucketAngle=Mathf.Clamp(m.bucketAngle+y*45*dt,-100,75);if(Input.GetKeyDown(KeyCode.L))m.bucket.Toggle();}
        if(m.kind==ConstructionMachine.Kind.Bulldozer)m.bladeHeight=Mathf.Clamp(m.bladeHeight+r*dt,.55f,1.8f);
        if(m.kind==ConstructionMachine.Kind.Truck&&Input.GetKeyDown(KeyCode.Z))m.dumping=!m.dumping;
        if(m.kind==ConstructionMachine.Kind.Crane){m.hoist.yaw+=q*30*dt;m.hoist.drop=Mathf.Clamp(m.hoist.drop-r*2*dt,1,11.2f);m.hoist.reach=Mathf.Clamp(m.hoist.reach+t*2*dt,2,12);if(Input.GetKeyDown(KeyCode.L))m.hoist.Toggle();}
    }
    float Axis(KeyCode positive,KeyCode negative)=>(Input.GetKey(positive)?1:0)-(Input.GetKey(negative)?1:0);
    void LateUpdate(){if(detail){var p=machines[selected].transform.position;detail.transform.position=p+new Vector3(11,9,-13);detail.transform.LookAt(p+Vector3.up*2);}}
    void OnGUI()
    {
        var m=machines[selected];GUI.Box(new Rect(16,92,770,132),"CONSTRUCTION SITE | 1 dozer  2 excavator  3 truck  4 crane | selected: "+m.kind);
        GUI.Label(new Rect(28,117,740,76),"Arrows drive / steer | Q/E slew | R/F boom, blade or hoist | T/G stick or trolley\nY/H bucket angle | L bucket retention / release or crane hook | Z truck tipper\nF5 site overview, F6 selected machine. Excavator: collect, lift, slew right, release above truck.");
        string state=m.kind==ConstructionMachine.Kind.Excavator?$"Slew {m.yaw:F0} | Boom {m.boomAngle:F0} | Stick {m.stickAngle:F0} | Bucket {m.bucketAngle:F0} | Held {m.bucket.contents.Count}":m.kind==ConstructionMachine.Kind.Crane?$"Slew {m.hoist.yaw:F0} | Trolley {m.hoist.reach:F1} m | Cable {m.hoist.drop:F1} m | Hook {(m.hoist.joint?"attached":"free")}":m.kind==ConstructionMachine.Kind.Truck?$"Tipper {m.bedAngle:F0} degrees":$"Blade {m.bladeHeight:F2} m";
        GUI.Label(new Rect(28,197,740,24),state);
    }
}

