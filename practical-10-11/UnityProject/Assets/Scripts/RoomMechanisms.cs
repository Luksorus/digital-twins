using UnityEngine;

public class RoomMechanisms : MonoBehaviour
{
    public Camera eyes;
    public RoomFan fan;
    public WindowBlinds blinds;
    public bool UseDoor()
    {
        if(!Physics.Raycast(eyes.transform.position,eyes.transform.forward,out var hit,3.5f,~0,QueryTriggerInteraction.Ignore))return false;
        var door=hit.collider.GetComponentInParent<HingedDoor>();if(!door)return false;door.Toggle();return true;
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))UseDoor();
        if(Input.GetKeyDown(KeyCode.F))fan.Toggle();
        if(Input.GetKeyDown(KeyCode.B))blinds.Toggle();
    }
    void OnGUI(){if(eyes.enabled)GUI.Box(new Rect(16,158,620,44),"O aim at door and open/close | F fan on/off | B blinds\nE take visible item (cabinet door must be open), R release");}
}
