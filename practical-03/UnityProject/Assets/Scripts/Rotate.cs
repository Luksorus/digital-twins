using UnityEngine;
public class Rotate : MonoBehaviour
{
    public float speed = 60f;
    public bool automatic;
    void Update()
    {
        Vector3 axes = automatic ? Vector3.one : new Vector3(
            Input.GetKey(KeyCode.X) ? 1 : 0,
            Input.GetKey(KeyCode.Y) ? 1 : 0,
            Input.GetKey(KeyCode.Z) ? 1 : 0);
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) axes = -axes;
        Step(axes, Time.deltaTime);
    }
    public void Step(Vector3 axes, float dt)
    {
        transform.localRotation = Quaternion.Euler(transform.localEulerAngles + axes * speed * dt);
    }
}
