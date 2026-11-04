using UnityEngine;

public class CityCameraRig : MonoBehaviour
{
    public Camera[] cameras;
    public int activeCamera;
    public bool showHelp = true;
    public void Select(int index)
    {
        activeCamera = Mathf.Clamp(index, 0, cameras.Length - 1);
        for (int i = 0; i < cameras.Length; i++) cameras[i].enabled = i == activeCamera;
    }
    void Start() { Select(activeCamera); }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) Select(0);
        if (Input.GetKeyDown(KeyCode.F2)) Select(1);
        if (Input.GetKeyDown(KeyCode.F3)) Select(2);
        if (Input.GetKeyDown(KeyCode.F4)) Select(3);
        if (Input.GetKeyDown(KeyCode.F5) && cameras.Length>4) Select(4);
        if (Input.GetKeyDown(KeyCode.H)) showHelp = !showHelp;
    }
    void OnGUI()
    {
        if (!showHelp) return;
        GUI.Box(new Rect(16,16,620,68), "DIGITAL TWINS  |  VIRTUAL CITY");
        GUI.Label(new Rect(28,40,590,42), "F1 city  F2 walk  F3 station  F4 map  H hide help\nWASD walk, Left/Right arrows turn. Buildings have solid colliders.");
    }
}

