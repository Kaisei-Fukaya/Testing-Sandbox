using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEditor;
using SSL.Data.Utils;
using SSL.Data;

namespace SSL.Graph
{
    public class PreviewBox : VisualElement
    {
        Editor _previewEditor;
        SwordGraph _swordGraph;
        Mesh _mesh;
        GameObject _previewObject;
        MeshRenderer _previewMR;
        MeshFilter _previewMF;

        public void Initialise(GraphView graphView, GAGenData data, Material[] materials)
        {
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetPreviewStyle.uss", typeof(StyleSheet)));

            //var previewObjectTemplate = (GameObject)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/PreviewTemplate.prefab", typeof(GameObject));
            _previewObject = new GameObject();
            _previewMR = _previewObject.AddComponent<MeshRenderer>();
            _previewMF = _previewObject.AddComponent<MeshFilter>();
            _previewObject.hideFlags = HideFlags.HideInHierarchy;

            VisualElement topContainer = new VisualElement()
            {
                name = "topContainer"
            };
            VisualElement mainContainer = new VisualElement()
            {
                name = "mainContainer"
            };

            TextElement titleElement = new TextElement()
            {
                text = "Preview",
                name = "titleElement"
            };

            IMGUIContainer previewContainer = new IMGUIContainer(() =>
            {
                var guiStyle = new GUIStyle();
                //guiStyle.normal.background = Texture2D.blackTexture;
                _previewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(0, 500, 0, 500), guiStyle);
            }
            );

            topContainer.Add(titleElement);
            mainContainer.Add(previewContainer);

            contentContainer.Add(topContainer);
            contentContainer.Add(mainContainer);


            RegisterCallback<DragUpdatedEvent>(e =>
            {
                //e.StopPropagation();
            });

            RegisterCallback<WheelEvent>(e =>
            {
                //e.StopPropagation();
            });

            RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == (int)MouseButton.LeftMouse)
                    graphView.ClearSelection();
                // prevent ContentDragger manipulator
                //e.StopPropagation();
            });


            focusable = true;

            _swordGraph = new SwordGraph();
            UpdateMesh(data, 2, 0f, materials);
        }

        public void UpdateMesh(GAGenData data, int subdiv, float spacing, Material[] materials)
        {
            GAGenData.NodesAndEdges nodesAndEdges = data.GetNodesAndEdges(subdiv);
            _swordGraph.Load(subdiv, spacing, nodesAndEdges.nodes, nodesAndEdges.edges, false);
            if (_previewEditor == null)
            {
                _mesh = _swordGraph.Generate();
                _previewMF.mesh = _mesh;
                _previewMR.sharedMaterials = materials;
                _previewEditor = Editor.CreateEditor(_previewObject);
                return;
            }
            if(_previewMR.sharedMaterials != materials)
                _previewMR.sharedMaterials = materials;
            _swordGraph.Generate(ref _mesh);
            _previewEditor.ReloadPreviewInstances();
        }
    }
}