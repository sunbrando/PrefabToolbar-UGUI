using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;


public class Menu : MonoBehaviour {

    [MenuItem("NGUI/Open/Prefab Toolbar", false, 9)]
    static public void OpenPrefabTool()
    {
        EditorWindow.GetWindow<UIPrefabTool>(false, "Prefab Toolbar", true).Show();
    }
}
