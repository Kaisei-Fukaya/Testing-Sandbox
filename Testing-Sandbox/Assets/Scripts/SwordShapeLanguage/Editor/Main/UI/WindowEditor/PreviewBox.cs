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

            _previewObject = (GameObject)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/PreviewTemplate.prefab", typeof(GameObject));
            //_previewObject = new GameObject();
            _previewMR = _previewObject.GetComponent<MeshRenderer>();
            _previewMF = _previewObject.GetComponent<MeshFilter>();
            //_previewObject.hideFlags = HideFlags.HideInHierarchy;

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
            UpdateMesh(data, 2, 0f, materials, false);
        }

        public void UpdateMesh(GAGenData data, int subdiv, float spacing, Material[] materials, bool facetedShading)
        {
            GAGenData.NodesAndEdges nodesAndEdges = data.GetNodesAndEdges(subdiv);
            _swordGraph.Load(subdiv, spacing, nodesAndEdges.nodes, nodesAndEdges.edges, facetedShading);
            if (_previewEditor == null)
            {
                _mesh = _swordGraph.Generate();
                _previewMF.mesh = _mesh;
                _previewMR.sharedMaterials = materials;
                _previewEditor = Editor.CreateEditor(_previewObject);
                return;
            }
            //if (_previewMR.sharedMaterials != materials)
            //{
            //    if (materials.Length > 0)
            //        Debug.Log(materials[0]);
            //    if(materials.Length > 1)
            //        Debug.Log(materials[1]);
            //}
            _previewMR.sharedMaterials = materials;
            _swordGraph.Generate(ref _mesh);
            _previewEditor.ReloadPreviewInstances();
        }
    }
}