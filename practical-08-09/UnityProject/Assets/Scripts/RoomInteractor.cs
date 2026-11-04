using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RoomInteractor : MonoBehaviour
{
    public Camera eyes;
    public float reach=3.5f;
    public Rigidbody held;
    Collider heldCollider,playerCollider;
    void Awake(){playerCollider=GetComponent<Collider>();}
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))Interact();
        if(Input.GetKeyDown(KeyCode.R))Drop();
        if(held)MoveHeld();
    }
    public bool Interact()
    {
        if(!Physics.Raycast(eyes.transform.position,eyes.transform.forward,out var hit,reach,~0,QueryTriggerInteraction.Ignore))return false;
        var lightSwitch=hit.collider.GetComponentInParent<RoomLightSwitch>();
        if(lightSwitch){lightSwitch.Toggle();return true;}
        var item=hit.collider.GetComponentInParent<PortableItem>();return item&&Take(item);
    }
    public bool Take(PortableItem item)
    {
        if(!item||!item.CanTake||Vector3.Distance(eyes.transform.position,item.transform.position)>reach)return false;
        // Direct calls use the same line-of-sight restriction as a keyboard interaction.
        Vector3 ray=item.transform.position-eyes.transform.position;
        if(Physics.Raycast(eyes.transform.position,ray.normalized,out var hit,ray.magnitude+.05f,~0,QueryTriggerInteraction.Ignore)&&hit.rigidbody!=item.GetComponent<Rigidbody>())return false;
        Drop();held=item.GetComponent<Rigidbody>();heldCollider=held.GetComponent<Collider>();
        if(!playerCollider)playerCollider=GetComponent<Collider>();
        held.linearVelocity=Vector3.zero;held.isKinematic=true;
        Physics.IgnoreCollision(heldCollider,playerCollider,true);return true;
    }
    public void MoveHeld()
    {
        Vector3 target=eyes.transform.position+eyes.transform.forward*1.5f;
        Vector3 delta=target-held.position;
        if(held.SweepTest(delta.normalized,out var hit,delta.magnitude,QueryTriggerInteraction.Ignore))delta=delta.normalized*Mathf.Max(0,hit.distance-.06f);
        held.position+=delta;held.rotation=Quaternion.Lerp(held.rotation,eyes.transform.rotation,Time.deltaTime*8);
    }
    public void Drop()
    {
        if(!held)return;
        Physics.IgnoreCollision(heldCollider,playerCollider,false);held.isKinematic=false;
        held.WakeUp();held=null;heldCollider=null;
    }
    void OnDisable(){Drop();}
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var body=hit.rigidbody;if(!body||body.isKinematic||hit.moveDirection.y<-.3f)return;
        body.AddForce(new Vector3(hit.moveDirection.x,0,hit.moveDirection.z)*35f,ForceMode.Force);
    }
    void OnGUI()
    {
        if(!eyes||!eyes.enabled)return;
        GUI.Label(new Rect(Screen.width/2-5,Screen.height/2-12,25,25),"+");
        GUI.Box(new Rect(16,100,620,54),"ROOM: E interact / take, R release. Walk into portals.\nWASD move, arrows turn, PgUp/PgDn look. Push furniture by walking.");
    }
}
