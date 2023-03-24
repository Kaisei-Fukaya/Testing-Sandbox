using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using UnityEditor.Experimental.GraphView;
using SSL.Data;
using SSL.Data.Utils;

namespace SSL.Graph
{
    public class VectorGraphicToolWindow : EditorWindow
    {
        StyleSheet _variablesStyleSheet;
        StyleSheet _generateStyleVariables, _trainStyleVariables;
        StyleSheet _toolbarToggleStyles;
        GraphicalAssetGraphView _graphView;
        PreviewBox _previewWindow;
        VisualElement _mainView;
        public bool inTrainingMode;

        GAGenData _saveData;

        [MenuItem("Window/Vector Editor")]
        public static void ShowWindow()
        {
            GetWindow<VectorGraphicToolWindow>();
        }

        private void CreateGUI()
        {
            this.titleContent = new GUIContent("Vector Editor");
            _generateStyleVariables = (StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetGeneratorVariablesGenerate.uss", typeof(StyleSheet));
            _mainView = new VisualElement()
            {
                name = "mainView"
            };
            _mainView.AddToClassList("root");
            //mainView.StretchToParentSize();
            rootVisualElement.Add(_mainView);
            AddToolbar();
        }


        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            ToolbarButton saveButton = new ToolbarButton()
            {
                text = "Save"
            };
            saveButton.clicked += Save;

            ToolbarButton saveAsButton = new ToolbarButton()
            {
                text = "Save As"
            };
            saveAsButton.clicked += SaveAs;

            ToolbarButton loadButton = new ToolbarButton()
            {
                text = "Load"
            };
            loadButton.clicked += Load;

            ToolbarSpacer spacer1 = new ToolbarSpacer();
            ToolbarSpacer spacer2 = new ToolbarSpacer();
            ToolbarSpacer spacer3 = new ToolbarSpacer();
            toolbar.Add(saveButton);
            toolbar.Add(spacer1);
            toolbar.Add(saveAsButton);
            toolbar.Add(loadButton);
            toolbar.Add(spacer2);
            toolbar.Add(spacer3);

            rootVisualElement.Add(toolbar);
        }

        void Save()
        {

        }

        void SaveAs()
        {

        }

        void Load()
        {

        }
    }

}