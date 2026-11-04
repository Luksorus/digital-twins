using UnityEngine;
public class Move : MonoBehaviour
{
    public float speed = 2f;
    public bool automatic;
    void Update()
    {
        Vector3 direction = automatic ? Vector3.right : new Vector3(
            (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
            (Input.GetKey(KeyCode.E) ? 1 : 0) - (Input.GetKey(KeyCode.Q) ? 1 : 0),
            (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0));
        Step(direction, Time.deltaTime);
    }
    public void Step(Vector3 direction, float dt)
    {
        transform.position += Vector3.ClampMagnitude(direction, 1f) * speed * dt;
    }
}
