using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MarchingCubesTestController))]
public class MarchingCubesTestControllerEditor : Editor
{
    MarchingCubesTestController _controller;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!_controller) { _controller = (MarchingCubesTestController)target; }

        if (GUILayout.Button("Generate Gradient"))
        {
            _controller.GenerateGradient();
        }

        if (GUILayout.Button("Generate Random"))
        {
            _controller.GenerateRand();
        }

        if (GUILayout.Button("Generate Perlin"))
        {
            _controller.GeneratePerlin();
        }
    }
}