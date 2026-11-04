using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CityWalker : MonoBehaviour
{
    public float speed = 4f;
    public float turnSpeed = 100f;
    float verticalVelocity;
    CharacterController controller;
    void Awake() { controller = GetComponent<CharacterController>(); }
    void Update()
    {
        float yaw = (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
        transform.Rotate(0f, yaw * turnSpeed * Time.deltaTime, 0f);
        float x = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
        float z = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
        Step(new Vector3(x, 0f, z), Time.deltaTime);
    }
    public CollisionFlags Step(Vector3 direction, float dt)
    {
        if (!controller) controller = GetComponent<CharacterController>();
        verticalVelocity = controller.isGrounded ? -1f : verticalVelocity + Physics.gravity.y * dt;
        Vector3 movement = transform.TransformDirection(Vector3.ClampMagnitude(direction, 1f)) * speed;
        movement.y = verticalVelocity;
        return controller.Move(movement * dt);
    }
}
