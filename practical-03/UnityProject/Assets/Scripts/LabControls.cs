using UnityEngine;
public class LabControls : MonoBehaviour
{
    public GameObject cube, cylinder, capsule, sphere;
    Vector3[] positions, scales;
    Quaternion[] rotations;
    GameObject[] objects;
    bool automatic;
    void Start()
    {
        objects = new[] { cube, cylinder, capsule, sphere };
        positions = new Vector3[4]; scales = new Vector3[4]; rotations = new Quaternion[4];
        for (int i=0;i<4;i++) { positions[i]=objects[i].transform.position; scales[i]=objects[i].transform.localScale; rotations[i]=objects[i].transform.localRotation; }
        for(int i=1;i<Display.displays.Length;i++) Display.displays[i].Activate();
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab)) { automatic=!automatic; foreach(var o in objects) { var m=o.GetComponent<Move>(); if(m) m.automatic=automatic; var s=o.GetComponent<Scale>(); if(s) s.automatic=automatic; var r=o.GetComponent<Rotate>(); if(r) r.automatic=automatic; } }
        if(Input.GetKeyDown(KeyCode.Backspace)) for(int i=0;i<4;i++) { objects[i].transform.position=positions[i]; objects[i].transform.localScale=scales[i]; objects[i].transform.localRotation=rotations[i]; }
    }
    void OnGUI()
    {
        GUI.Box(new Rect(16,16,690,112), "PRACTICAL 3  |  " + (automatic ? "AUTOMATIC" : "KEYBOARD"));
        GUI.Label(new Rect(30,43,660,80), "Cube + Sphere: arrows move X/Z, E/Q move up/down\nCylinder + Sphere: R/F scale X, T/G scale Y, V/B scale Z\nCapsule + Sphere: X/Y/Z rotate, Shift reverses rotation\nTab: automatic / keyboard     Backspace: reset transforms");
    }
}
