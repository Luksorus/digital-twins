using UnityEngine;

public class WindowBlinds : MonoBehaviour
{
    public Transform[] slats;
    public float openness;
    public bool targetOpen;
    public void Toggle(){targetOpen=!targetOpen;}
    public void Step(float dt)
    {
        openness=Mathf.MoveTowards(openness,targetOpen?1:0,dt);
        for(int i=0;i<slats.Length;i++){
            slats[i].localPosition=new Vector3(0,Mathf.Lerp(1.2f+i*.19f,3.1f+i*.025f,openness),0);
            slats[i].localRotation=Quaternion.Euler(0,0,openness*75);
        }
    }
    void FixedUpdate(){Step(Time.fixedDeltaTime);}
}
