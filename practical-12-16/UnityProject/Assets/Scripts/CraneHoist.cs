using UnityEngine;

public class CraneHoist : MonoBehaviour
{
    public Rigidbody hook;
    public Transform slew,trolley;
    public LineRenderer cable;
    public float yaw,reach=6,drop=10.5f;
    public FixedJoint joint;
    public void Toggle()
    {
        if(joint){Destroy(joint);joint=null;return;}
        foreach(var hit in Physics.OverlapSphere(hook.position,1.2f)){
            var load=hit.GetComponent<LiftableLoad>();if(!load)continue;
            joint=load.gameObject.AddComponent<FixedJoint>();joint.connectedBody=hook;joint.breakForce=50000;joint.breakTorque=50000;return;
        }
    }
    public void Pose(bool immediate=false)
    {
        slew.localRotation=Quaternion.Euler(0,yaw,0);trolley.localPosition=new Vector3(reach,0,0);
        Vector3 p=trolley.position-Vector3.up*drop;
        if(immediate)hook.position=p;else hook.MovePosition(p);
        cable.positionCount=2;cable.SetPosition(0,trolley.position);cable.SetPosition(1,p);
    }
    void FixedUpdate(){Pose();}
}
