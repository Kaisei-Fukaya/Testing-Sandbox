using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class MaterialsPopupWindow : EditorWindow
{

    public static void ShowWindow()
    {
        GetWindow<MaterialsPopupWindow>("Material Editor");
    }

    public void OnGUI()
    {

    }
}
