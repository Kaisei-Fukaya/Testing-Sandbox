using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GraphReader))]
public class GraphReaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GraphReader graphReader = (GraphReader)target;
        if (DrawDefaultInspector())
        {

        }
    }

    public void TriggerScript()
    {
        GraphReader graphReader = (GraphReader)target;
        if (graphReader.data.UpdateRequiredFlag)
        {
            graphReader.UpdateMesh();
            graphReader.data.UpdateRequiredFlag = false;
        }
    }

    public void OnEnable()
    {
        EditorApplication.update += TriggerScript;
    }

    public void OnDisable()
    {
        EditorApplication.update -= TriggerScript;
    }

}
