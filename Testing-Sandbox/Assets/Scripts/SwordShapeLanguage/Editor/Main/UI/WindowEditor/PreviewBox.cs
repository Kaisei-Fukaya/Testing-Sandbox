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

        public void Initialise(GraphView graphView, GAGenData data)
        {
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetPreviewStyle.uss", typeof(StyleSheet)));

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
            UpdateMesh(data);
        }

        public void UpdateMesh(GAGenData data)
        {
            GAGenData.NodesAndEdges nodesAndEdges = data.GetNodesAndEdges(2);
            _swordGraph.Load(2, 1f, nodesAndEdges.nodes, nodesAndEdges.edges, false);
            if (_previewEditor == null)
            {
                _mesh = _swordGraph.Generate();
                _previewEditor = Editor.CreateEditor(_mesh);
                return;
            }
            _swordGraph.Generate(ref _mesh);
        }
    }
}