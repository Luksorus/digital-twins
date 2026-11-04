using UnityEngine;

public class StationTrain : MonoBehaviour
{
    public enum Phase {Approach,Braking,Dwell,Depart}
    public Phase phase;
    public float positionX=-70f,speed,stopX=18f,maxSpeed=10f,acceleration=1.5f,braking=1.5f,dwellDuration=6f;
    public float dwellRemaining;
    public int stops,departures,cycles;
    public float tripTime;
    public Transform[] wheels;
    public void ResetTrip(){tripTime=0;positionX=-70;speed=0;phase=Phase.Approach;dwellRemaining=0;Apply();}
    void Apply(){var p=new Vector3(positionX,.47f,40);var body=GetComponent<Rigidbody>();if(body&&Application.isPlaying)body.MovePosition(p);else transform.position=p;}
    public void Step(float dt)
    {
        tripTime+=dt;Phase oldPhase=phase;
        float accelerationTime=maxSpeed/acceleration,brakingTime=maxSpeed/braking;
        float accelerationDistance=maxSpeed*maxSpeed/(2*acceleration);
        float brakingDistance=maxSpeed*maxSpeed/(2*braking);
        float cruiseTime=(stopX+70-accelerationDistance-brakingDistance)/maxSpeed;
        float arrival=accelerationTime+cruiseTime+brakingTime;
        if(tripTime<accelerationTime){phase=Phase.Approach;speed=acceleration*tripTime;positionX=-70+.5f*acceleration*tripTime*tripTime;}
        else if(tripTime<accelerationTime+cruiseTime){phase=Phase.Approach;speed=maxSpeed;positionX=-70+accelerationDistance+maxSpeed*(tripTime-accelerationTime);}
        else if(tripTime<arrival){phase=Phase.Braking;float remaining=arrival-tripTime;speed=braking*remaining;positionX=stopX-.5f*braking*remaining*remaining;}
        else if(tripTime<arrival+dwellDuration){phase=Phase.Dwell;speed=0;positionX=stopX;dwellRemaining=arrival+dwellDuration-tripTime;}
        else{phase=Phase.Depart;float t=tripTime-arrival-dwellDuration;speed=Mathf.Min(maxSpeed,acceleration*t);positionX=stopX+(t<accelerationTime?.5f*acceleration*t*t:accelerationDistance+maxSpeed*(t-accelerationTime));}
        if(oldPhase!=Phase.Dwell&&phase==Phase.Dwell)stops++;
        if(oldPhase==Phase.Dwell&&phase==Phase.Depart)departures++;
        if(positionX>115){cycles++;ResetTrip();return;}
        float degrees=speed*dt/.45f*Mathf.Rad2Deg;
        if(wheels!=null)foreach(var wheel in wheels)wheel.Rotate(Vector3.up,degrees,Space.Self);
        Apply();
    }
}
