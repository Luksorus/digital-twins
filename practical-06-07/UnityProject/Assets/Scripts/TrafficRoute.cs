using UnityEngine;
using System.Collections.Generic;

public class TrafficRoute : MonoBehaviour
{
    public Vector3[] samples;
    public float[] cumulative;
    public float Length => cumulative[cumulative.Length-1];
    public void Build()
    {
        var points=new List<Vector3>();
        void Line(Vector2 a,Vector2 b){int n=Mathf.CeilToInt(Vector2.Distance(a,b)/.5f);for(int i=0;i<n;i++){var p=Vector2.Lerp(a,b,(float)i/n);points.Add(new Vector3(p.x,.155f,p.y));}}
        void Arc(Vector2 center,float start){for(int i=0;i<32;i++){float a=(start+i*90f/32)*Mathf.Deg2Rad;points.Add(new Vector3(center.x+8*Mathf.Cos(a),.155f,center.y+8*Mathf.Sin(a)));}}
        Line(new Vector2(-26,-26),new Vector2(26,-26));Arc(new Vector2(26,-18),-90);
        Line(new Vector2(34,-18),new Vector2(34,18));Arc(new Vector2(26,18),0);
        Line(new Vector2(26,26),new Vector2(-26,26));Arc(new Vector2(-26,18),90);
        Line(new Vector2(-34,18),new Vector2(-34,-18));Arc(new Vector2(-26,-18),180);
        points.Add(points[0]);samples=points.ToArray();cumulative=new float[samples.Length];
        for(int i=1;i<samples.Length;i++)cumulative[i]=cumulative[i-1]+Vector3.Distance(samples[i-1],samples[i]);
    }
    public void Pose(float distance,out Vector3 position,out Quaternion rotation)
    {
        distance=Mathf.Repeat(distance,Length);int low=0,high=cumulative.Length-1;
        while(high-low>1){int middle=(low+high)/2;if(cumulative[middle]<=distance)low=middle;else high=middle;}
        float t=(distance-cumulative[low])/(cumulative[high]-cumulative[low]);
        position=Vector3.Lerp(samples[low],samples[high],t);rotation=Quaternion.LookRotation(samples[high]-samples[low]);
    }
    public float SpeedLimit(float distance)
    {
        Pose(distance-2,out _,out var a);Pose(distance+2,out _,out var b);
        return Quaternion.Angle(a,b)>6f?4f:8f;
    }
}
