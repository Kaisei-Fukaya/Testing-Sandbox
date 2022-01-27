using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MCCellularAutomationController))]
public class MCCellularAutomationEditor : Editor
{
    MCCellularAutomationController _controller;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!_controller) { _controller = (MCCellularAutomationController)target; }

        if (GUILayout.Button("Go"))
        {
            _controller.StartAutomation();
        }
    }
}
