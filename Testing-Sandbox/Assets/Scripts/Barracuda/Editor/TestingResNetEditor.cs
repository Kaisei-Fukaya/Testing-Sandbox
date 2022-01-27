using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(TestingResNet))]
public class TestingResNetEditor : Editor
{
    TestingResNet _target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!_target) { _target = (TestingResNet)target; }

        //Handle option dropdown
        if (_target.models.Length != 0)
        {
            string[] options = new string[_target.models.Length];
            for (int i = 0; i < _target.models.Length; i++)
            {
                if(_target.models[i] == null) { return; }
                options[i] = _target.models[i].name;
            }

            _target.selectedModelIndex = EditorGUILayout.Popup("Model", _target.selectedModelIndex, options);
            if (_target.activeModel != _target.models[_target.selectedModelIndex]) { _target.activeModel = _target.models[_target.selectedModelIndex]; }
        }

        if (GUILayout.Button("Run"))
        {
            _target.Run();
        }
    }
}