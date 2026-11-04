using UnityEngine;

public class RoomFan : MonoBehaviour
{
    public Transform rotor;
    public bool running;
    public float strength=1.8f,range=5,width=.8f,rotation;
    public Vector3 Direction=>transform.right;
    public Vector3 Outlet=>transform.position+Direction*.4f;
    public void Toggle(){running=!running;}
    void FixedUpdate()
    {
        if(!running)return;
        rotation=Mathf.Repeat(rotation+900*Time.fixedDeltaTime,360);
        rotor.localRotation=Quaternion.AngleAxis(rotation,Vector3.right);
        foreach(var item in FindObjectsByType<PortableItem>(FindObjectsSortMode.None))
        {
            var body=item.GetComponent<Rigidbody>();if(body.isKinematic||body.mass>1)continue;
            Vector3 delta=body.worldCenterOfMass-Outlet;float forward=Vector3.Dot(delta,Direction);
            if(forward<=0||forward>range||(delta-Direction*forward).magnitude>width)continue;
            if(Physics.Raycast(Outlet,delta.normalized,out var hit,delta.magnitude+.02f,~0,QueryTriggerInteraction.Ignore)&&hit.rigidbody!=body)continue;
            body.AddForce(Direction*strength*(1-forward/range),ForceMode.Force);
        }
    }
}
