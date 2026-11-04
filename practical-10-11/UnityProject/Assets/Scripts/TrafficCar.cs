using UnityEngine;

[RequireComponent(typeof(Rigidbody),typeof(BoxCollider))]
public class TrafficCar : MonoBehaviour
{
    public TrafficRoute route;
    public float distance,speed;
    public float acceleration=2f,braking=4f;
    Rigidbody body;
    public void Place(float at)
    {
        distance=at;route.Pose(distance,out var p,out var q);
        transform.SetPositionAndRotation(p,q);body=GetComponent<Rigidbody>();body.position=p;body.rotation=q;
    }
    public void Step(float dt,float targetSpeed,bool manual)
    {
        float previous=speed;speed=Mathf.MoveTowards(speed,Mathf.Max(0,targetSpeed),(targetSpeed<speed?braking:acceleration)*dt);
        distance=Mathf.Repeat(distance+(previous+speed)*.5f*dt,route.Length);
        route.Pose(distance,out var p,out var rotation);
        if(!body)body=GetComponent<Rigidbody>();
        if(manual){body.position=p;body.rotation=rotation;transform.SetPositionAndRotation(p,rotation);}
        else {body.MovePosition(p);body.MoveRotation(rotation);}
    }
}
