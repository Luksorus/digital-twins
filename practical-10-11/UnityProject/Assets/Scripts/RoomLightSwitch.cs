using UnityEngine;

public class RoomLightSwitch : MonoBehaviour
{
    public Light[] lights;
    public bool isOn=true;
    public Transform rocker;
    public void Toggle(){Set(!isOn);}
    public void Set(bool value)
    {
        isOn=value;foreach(var light in lights)light.enabled=value;
        if(rocker)rocker.localRotation=Quaternion.Euler(value?-18:18,0,0);
    }
}
