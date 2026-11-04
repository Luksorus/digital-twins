using UnityEngine;

public class PedestrianMover : MonoBehaviour
{
    public Vector3 start,end;
    public float speed=1.35f,progress;
    public int direction=1;
    public bool crossing;
    public Transform leftLeg,rightLeg,leftArm,rightArm;
    float waitTime,phase;
    public void Step(float dt,bool crossingAllowed)
    {
        float length=Vector3.Distance(start,end);
        bool atEntrance=progress<.001f||progress>length-.001f;
        bool moving=true;
        if(waitTime>0){waitTime-=dt;moving=false;}
        else if(crossing && atEntrance && !crossingAllowed)moving=false;
        if(moving){progress+=direction*speed*dt;if(progress>=length){progress=length;direction=-1;waitTime=2;}else if(progress<=0){progress=0;direction=1;waitTime=2;}}
        Vector3 forward=(end-start).normalized*direction;
        transform.position=Vector3.Lerp(start,end,progress/length);
        transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(forward),180f*dt);
        phase+=moving?dt*7:0;float angle=moving?Mathf.Sin(phase)*25f:0;
        if(leftLeg)leftLeg.localRotation=Quaternion.Euler(angle,0,0);
        if(rightLeg)rightLeg.localRotation=Quaternion.Euler(-angle,0,0);
        if(leftArm)leftArm.localRotation=Quaternion.Euler(-angle,0,0);
        if(rightArm)rightArm.localRotation=Quaternion.Euler(angle,0,0);
    }
}
