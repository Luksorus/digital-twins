using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class LabViews
{
    static void Open(int lab)
    {
        if(!EditorApplication.isPlaying) EditorSceneManager.OpenScene("Assets/Scenes/Practical0"+lab+".unity");
    }
    static void Game(int display)
    {
        var t=typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        var w=EditorWindow.GetWindow(t);w.Show();w.Focus();
        var p=t.GetProperty("targetDisplay",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        if(p!=null)p.SetValue(w,display);else{var s=new SerializedObject(w);var f=s.FindProperty("m_TargetDisplay");if(f!=null){f.intValue=display;s.ApplyModifiedProperties();}}
        w.Repaint();
    }
    [MenuItem("Practicals/Views/Lab 2 Scene #F6")]
    public static void Scene2(){Open(2);var s=SceneView.GetWindow<SceneView>();s.Show();s.Focus();s.LookAt(new Vector3(0,.7f,0),Quaternion.Euler(35,0,0),16);Selection.activeGameObject=GameObject.Find("Cube");}
    [MenuItem("Practicals/Views/Display 1 #F7")]
    public static void Display1(){Game(0);}
    [MenuItem("Practicals/Views/Display 2 #F8")]
    public static void Display2(){Game(1);}
    [MenuItem("Practicals/Views/Display 3 #F9")]
    public static void Display3(){Game(2);}
}
