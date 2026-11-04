using UnityEngine;
public class Scale : MonoBehaviour
{
    public float speed = 0.6f;
    public bool automatic;
    void Update()
    {
        Vector3 direction = automatic ? Vector3.one : new Vector3(
            (Input.GetKey(KeyCode.R) ? 1 : 0) - (Input.GetKey(KeyCode.F) ? 1 : 0),
            (Input.GetKey(KeyCode.T) ? 1 : 0) - (Input.GetKey(KeyCode.G) ? 1 : 0),
            (Input.GetKey(KeyCode.V) ? 1 : 0) - (Input.GetKey(KeyCode.B) ? 1 : 0));
        Step(direction, Time.deltaTime);
    }
    public void Step(Vector3 direction, float dt)
    {
        Vector3 s = transform.localScale + direction * speed * dt;
        transform.localScale = new Vector3(Mathf.Clamp(s.x, 0.2f, 4f), Mathf.Clamp(s.y, 0.2f, 4f), Mathf.Clamp(s.z, 0.2f, 4f));
    }
}
