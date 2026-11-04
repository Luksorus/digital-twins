using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PortableItem : MonoBehaviour
{
    public string label="Item";
    // An optional cabinet access condition is added in practical 10–11.
    public virtual bool CanTake => true;
}
