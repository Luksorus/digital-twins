using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ConstructionMachine : MonoBehaviour
{
    public enum Kind {Bulldozer,Excavator,Truck,Crane}
    public Kind kind;
    public Transform turret,boom,stick,bucketPivot,blade,bed,tailgate;
    public ExcavatorBucket bucket;
    public CraneHoist hoist;
    public float drive,steer,yaw,boomAngle=-25,stickAngle=85,bucketAngle=-60,bladeHeight=.55f,bedAngle;
    public bool dumping;
    public float speed=2;
    public void Pose()
    {
        if(turret)turret.localRotation=Quaternion.Euler(0,yaw,0);
        if(boom)boom.localRotation=Quaternion.Euler(boomAngle,0,0);
        if(stick)stick.localRotation=Quaternion.Euler(stickAngle,0,0);
        if(bucketPivot)bucketPivot.localRotation=Quaternion.Euler(bucketAngle,0,0);
        if(blade){var p=blade.localPosition;p.y=bladeHeight;blade.localPosition=p;}
        if(bed)bed.localRotation=Quaternion.Euler(-bedAngle,0,0);
        if(tailgate)tailgate.localRotation=Quaternion.Euler(bedAngle>8?-110:0,0,0);
    }
    void FixedUpdate()
    {
        float dt=Time.fixedDeltaTime;
        if(kind!=Kind.Crane){var rb=GetComponent<Rigidbody>();Vector3 p=rb.position+transform.forward*drive*speed*dt;
            p.x=Mathf.Clamp(p.x,-32,32);p.z=Mathf.Clamp(p.z,-96,-60);
            Vector3 delta=p-rb.position;
            // Stop against fixed site obstacles; dynamic grains remain pushable.
            bool blocked=false;
            if(delta.sqrMagnitude>.000001f)foreach(var hit in rb.SweepTestAll(delta.normalized,delta.magnitude+.02f,QueryTriggerInteraction.Ignore))
                if(hit.normal.y<.5f&&(!hit.rigidbody||hit.rigidbody.isKinematic)&&hit.rigidbody!=rb&&!hit.transform.IsChildOf(transform)&&hit.collider.gameObject.name!="SiteFloor")blocked=true;
            if(!blocked)rb.MovePosition(p);rb.MoveRotation(rb.rotation*Quaternion.Euler(0,steer*45*dt,0));
        }
        bedAngle=Mathf.MoveTowards(bedAngle,dumping?45:0,18*dt);Pose();
    }
}
