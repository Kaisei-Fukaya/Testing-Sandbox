using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RunOptimisation))]
public class RunOptimisationEditor : Editor
{
    RunOptimisation _controller;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!_controller) { _controller = (RunOptimisation)target; }

        if (GUILayout.Button("Go"))
        {
            _controller.Run();
        }
    }
}
