using System.Collections.Generic;
using UnityEngine;

public class ExcavatorBucket : MonoBehaviour
{
    public List<Rigidbody> contents=new List<Rigidbody>();
    public Vector3 center=new Vector3(0,-.3f,.7f),size=new Vector3(1.8f,.65f,1.35f);
    public int Collect()
    {
        if(contents.Count>0)return contents.Count;
        foreach(var hit in Physics.OverlapBox(transform.TransformPoint(center),size/2,transform.rotation,~0,QueryTriggerInteraction.Ignore))
        {
            var grain=hit.GetComponent<ConstructionGrain>();if(!grain)continue;var body=grain.GetComponent<Rigidbody>();if(body.isKinematic)continue;
            body.linearVelocity=Vector3.zero;body.angularVelocity=Vector3.zero;body.isKinematic=true;
            body.transform.SetParent(transform,true);contents.Add(body);
        }
        return contents.Count;
    }
    public void Release()
    {foreach(var body in contents){body.transform.SetParent(null,true);body.isKinematic=false;body.WakeUp();}contents.Clear();}
    public void Toggle(){if(contents.Count==0)Collect();else Release();}
}
