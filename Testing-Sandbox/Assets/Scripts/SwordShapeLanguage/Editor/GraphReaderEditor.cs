using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SSL.Graph;

[CustomEditor(typeof(GraphReader))]
public class GraphReaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GraphReader graphReader = (GraphReader)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Open In Graph Editor"))
        {
            GraphicalAssetGeneratorWindow window;
            if (EditorWindow.HasOpenInstances<GraphicalAssetGeneratorWindow>())
            {
                window = EditorWindow.CreateWindow<GraphicalAssetGeneratorWindow>();
            }
            else
            {
                EditorWindow.FocusWindowIfItsOpen<GraphicalAssetGeneratorWindow>();
                window = EditorWindow.GetWindow<GraphicalAssetGeneratorWindow>();
            }
            window.Load(graphReader.data);
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
