using UnityEngine;
using UnityEditor;

public class BuildMap : EditorWindow
{
    [MenuItem("Vectorier/BuildMap")]
    public static void Build()
    {
        Debug.Log("Building...");
    }
}
