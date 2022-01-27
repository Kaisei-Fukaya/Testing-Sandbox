using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MarchingCubesPaintController))]
public class MarchingCubesPaintControllerEditor : Editor
{
    MarchingCubesPaintController _controller;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!_controller) { _controller = (MarchingCubesPaintController)target; }

        if (GUILayout.Button("Generate Plane"))
        {
            _controller.GeneratePlane();
        }
    }
}