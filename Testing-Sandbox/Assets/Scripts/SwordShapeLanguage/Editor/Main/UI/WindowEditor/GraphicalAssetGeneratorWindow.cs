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
    public class GraphicalAssetGeneratorWindow : EditorWindow
    {
        StyleSheet _variablesStyleSheet;
        StyleSheet _generateStyleVariables, _trainStyleVariables;
        StyleSheet _toolbarToggleStyles;
        GraphicalAssetGraphView _graphView;
        VisualElement _mainView;
        public bool inTrainingMode;
        bool _updateFlag = false;

        GAGenData _saveData;

        [MenuItem("Window/Graphical Asset Generator")]
        public static void ShowWindow()
        {
            GetWindow<GraphicalAssetGeneratorWindow>();
        }

        private void CreateGUI()
        {
            this.titleContent = new GUIContent("Graphical Asset Generator");
            _generateStyleVariables = (StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetGeneratorVariablesGenerate.uss", typeof(StyleSheet));
            _trainStyleVariables = (StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetGeneratorVariablesTrain.uss", typeof(StyleSheet));
            _toolbarToggleStyles = (StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetToolbarToggleStyle.uss", typeof(StyleSheet));
            AddToolbar();
            _mainView = new VisualElement()
            {
                name = "mainView"
            };
            _mainView.AddToClassList("root");
            //mainView.StretchToParentSize();
            rootVisualElement.Add(_mainView);
            AddGraphView();
            AddStyles();
            //AddPreviewWindow();
            SetupDragAndDrop();
        }

        private void SetupDragAndDrop()
        {
            Color origColour = _mainView.style.backgroundColor.value;
            //Drag enter
            _mainView.RegisterCallback<DragEnterEvent>(e =>
            {
                _mainView.style.backgroundColor = new StyleColor(new Color(0f, 0f, 100f, 0.3f));
            });
            //Drag leave
            _mainView.RegisterCallback<DragLeaveEvent>(e =>
            {
                _mainView.style.backgroundColor = new StyleColor(origColour);
            });
            //Drag updated
            _mainView.RegisterCallback<DragUpdatedEvent>(e =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            });
            //Drag perform
            _mainView.RegisterCallback<DragPerformEvent>(e =>
            {
                var draggedObjects = DragAndDrop.objectReferences;

                for (int i = 0; i < draggedObjects.Length; i++)
                {
                    if (draggedObjects[i] is GAGenData)
                    {
                        Load((GAGenData)draggedObjects[i]);
                        break;
                    }
                }

                _mainView.style.backgroundColor = new StyleColor(origColour);
            });
            //Drag exited
            _mainView.RegisterCallback<DragExitedEvent>(e =>
            {
                _mainView.style.backgroundColor = new StyleColor(origColour);
            });
        }

        void AddGraphView()
        {
            _graphView = new GraphicalAssetGraphView(this);
            _graphView.StretchToParentSize();
            _mainView.Add(_graphView);
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

        private void AddPreviewWindow()
        {
            GAPreview previewWindow = new GAPreview();
            previewWindow.Initialise(_graphView, _saveData);
            previewWindow.name = "previewWindow";
            _mainView.Add(previewWindow);
        }

        void Save()
        {
            //If save data doesn't exist, call save as
            if (_saveData == null)
            {
                SaveAs();
                return;
            }

            //Otherwise overwrite the data
            SaveData(_graphView, _saveData);
            //AssetDatabase.SaveAsset(_saveData);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();

            _saveData.UpdateRequiredFlag = true;

            //void RefreshSelection()
            //{
            //    //if (Selection.activeObject == null)
            //    //{
            //    //    Selection.activeObject = _saveData;
            //    //    _saveData.UpdateRequiredFlag = true;
            //    //    GAGenDataUtils.RepaintInspector(typeof(GraphReaderEditor));
            //    //}
            //    //else
            //    //{
            //    //    Selection.activeObject = null;
            //    //    EditorApplication.delayCall += RefreshSelection;
            //    //}
            //}

            //EditorApplication.delayCall += RefreshSelection;
        }

        void SaveData(GraphicalAssetGraphView graphView, GAGenData saveData)
        {
            List<GraphViewNode> nodes = graphView.Nodes;
            saveData.Nodes = new List<GAGenNodeData>();
            foreach (GraphViewNode node in nodes)
            {
                saveData.Nodes.Add(GAGenDataUtils.GraphNodeToNodeData(node));
            }
            EditorUtility.SetDirty(saveData);
        }

        void SaveAs()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Save As", "New Graphical Asset Generator", "asset", "");
            if (savePath == string.Empty)
                return;
            _saveData = CreateInstance<GAGenData>();
            SaveData(_graphView, _saveData);
            AssetDatabase.CreateAsset(_saveData, savePath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = _saveData;
        }

        public void Load(GAGenData data)
        {
            if (data == null)
            {
                Debug.LogWarning("Selected file was not compatible");
                return;
            }

            _saveData = data;

            ClearGraph();

            Dictionary<string, NodeAndData> iDToNode = new Dictionary<string, NodeAndData>();
            List<string> allNodeIDs = new List<string>();
            foreach (GAGenNodeData nodeData in _saveData.Nodes)
            {
                GraphViewNode newNode = _graphView.CreateNode(nodeData.NodeType, nodeData.Position);
                newNode.ID = nodeData.ID;
                newNode.LoadSettings(nodeData.Settings);
                newNode.Draw();
                iDToNode.Add(newNode.ID, new NodeAndData(newNode, nodeData));
                allNodeIDs.Add(newNode.ID);
                _graphView.AddElement(newNode);
            }

            //Make sure this is done last so that all ports are drawn
            foreach (string id in allNodeIDs)
            {
                NodeAndData nodeAndData = iDToNode[id];
                GraphViewNode node = nodeAndData.node;
                GAGenNodeData nodeData = nodeAndData.data;
                List<GraphicalAssetPort> ports = node.GetPorts(true);
                if (nodeData.InGoingConnections == null)
                    continue;
                //Gen connections
                if (ports.Count != nodeData.InGoingConnections.Count)
                {
                    //Debug.Log("dddd");
                    continue;
                }
                for (int i = 0; i < nodeData.InGoingConnections.Count; i++)
                {
                    if (nodeData.InGoingConnections[i].iD == "EMPTY")
                        continue;

                    GraphViewNode otherNode = iDToNode[nodeData.InGoingConnections[i].iD].node;
                    List<GraphicalAssetPort> otherPorts = otherNode.GetPorts(false);
                    Edge edge = otherPorts[nodeData.InGoingConnections[i].indexInOther].ConnectTo(ports[i].GetPort());
                    if (edge == null)
                        continue;
                    _graphView.AddElement(edge);
                }
            }

            EditorApplication.delayCall += _graphView.CentreGraphOnNodes;
            _graphView.NodeUpdateFlag();
            titleContent = new GUIContent($"{_saveData.name} (Graphical Asset Generator)");
        }

        void Load()
        {
            string path = EditorUtility.OpenFilePanel("Load", "Assets", "asset");
            path = path.Replace(Application.dataPath, "Assets");
            GAGenData data = AssetDatabase.LoadAssetAtPath<GAGenData>(path);
            Load(data);
        }

        struct NodeAndData
        {
            public GraphViewNode node;
            public GAGenNodeData data;
            public NodeAndData(GraphViewNode node, GAGenNodeData data)
            {
                this.node = node;
                this.data = data;
            }
        }

        void ClearGraph()
        {
            _graphView.ClearGraph();
        }

        void SetStyle()
        {
            rootVisualElement.styleSheets.Remove(_variablesStyleSheet);
            if (inTrainingMode)
                _variablesStyleSheet = _trainStyleVariables;
            else
                _variablesStyleSheet = _generateStyleVariables;
            rootVisualElement.styleSheets.Add(_variablesStyleSheet);
        }

        void AddStyles()
        {
            _variablesStyleSheet = _generateStyleVariables;
            StyleSheet windowStyleSheet = (StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetGeneratorWindowStyle.uss", typeof(StyleSheet));
            rootVisualElement.styleSheets.Add(_variablesStyleSheet);
            rootVisualElement.styleSheets.Add(windowStyleSheet);
        }
    }

}