using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HingedDoor : MonoBehaviour
{
    public float angle,openAngle=105,speed=85;
    public bool targetOpen;
    public bool IsOpen => angle>=65;
    public void Toggle(){targetOpen=!targetOpen;}
    public void Step(float dt,bool immediate=false)
    {
        angle=Mathf.MoveTowards(angle,targetOpen?openAngle:0,speed*dt);
        var rotation=Quaternion.Euler(0,angle,0);
        if(immediate)transform.localRotation=rotation;
        else GetComponent<Rigidbody>().MoveRotation(transform.parent?transform.parent.rotation*rotation:rotation);
    }
    void FixedUpdate(){Step(Time.fixedDeltaTime);}
}
