using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEditor;
using SSL.Data.Utils;

namespace SSL.Graph
{
    public class GAPreview : GraphElement
    {
        Mesh _mesh;
        Editor _previewEditor;

        public void Initialise(GraphView graphView)
        {
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetPreviewStyle.uss", typeof(StyleSheet)));
            _mesh = Resources.GetBuiltinResource(typeof(Mesh), $"Cube.fbx") as Mesh;
            _previewEditor = Editor.CreateEditor(_mesh);


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
                    guiStyle.normal.background = Texture2D.blackTexture;
                    _previewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(0, 500, 0, 500), guiStyle);
                }
            );

            topContainer.Add(titleElement);
            mainContainer.Add(previewContainer);

            contentContainer.Add(topContainer);
            contentContainer.Add(mainContainer);

            capabilities |= Capabilities.Movable | Capabilities.Resizable;
            var dragger = new Dragger() { clampToParentEdges = true };
            this.AddManipulator(dragger);
            hierarchy.Add(new Resizer());

            RegisterCallback<DragUpdatedEvent>(e =>
            {
                e.StopPropagation();
            });

            RegisterCallback<WheelEvent>(e =>
            {
                e.StopPropagation();
            });

            RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.button == (int)MouseButton.LeftMouse)
                    graphView.ClearSelection();
                // prevent ContentDragger manipulator
                e.StopPropagation();
            });

            
            focusable = true;
        }
    }
}