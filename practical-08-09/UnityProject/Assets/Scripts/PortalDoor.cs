using UnityEngine;

// A shared cooldown prevents the destination trigger from teleporting straight back.
[RequireComponent(typeof(BoxCollider))]
public class PortalDoor : MonoBehaviour
{
    public Transform destination;
    public static float lastTeleport = -100;
    public bool Enter(CharacterController walker)
    {
        if (!destination || Time.time - lastTeleport < 1f) return false;
        var hands=walker.GetComponent<RoomInteractor>(); if(hands) hands.Drop();
        walker.enabled=false;
        walker.transform.SetPositionAndRotation(destination.position,destination.rotation);
        walker.enabled=true; lastTeleport=Time.time;
        var rig=FindFirstObjectByType<CityCameraRig>();if(rig)rig.Select(1);
        return true;
    }
    void OnTriggerEnter(Collider other)
    { var walker=other.GetComponent<CharacterController>();if(walker)Enter(walker); }
    void OnTriggerStay(Collider other)
    { var walker=other.GetComponent<CharacterController>();if(walker)Enter(walker); }
    void FixedUpdate()
    {
        // CharacterController trigger messages depend on a Move in the physics tick.
        // Query the same trigger volume as a fallback for teleports and editor-driven movement.
        var volume=GetComponent<BoxCollider>();
        foreach(var other in Physics.OverlapBox(transform.TransformPoint(volume.center),Vector3.Scale(volume.size,transform.lossyScale)*.5f,transform.rotation,~0,QueryTriggerInteraction.Collide))
        {var walker=other.GetComponent<CharacterController>();if(walker)Enter(walker);}
    }
}
