using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Linq;
using UnityEditor;
using SSL.Graph.Elements;
using SSL.Data.Utils;

namespace SSL.Graph
{
    public class GraphicalAssetGraphView : GraphView
    {
        GASearchWindow _searchWindow;
        public GraphicalAssetGeneratorWindow editorWindow;
        List<GraphViewNode> _nodes = new List<GraphViewNode>();
        public List<GraphViewNode> Nodes 
        { 
            get
            {
                return _nodes;
            } 
        }
        public GraphicalAssetGraphView(GraphicalAssetGeneratorWindow editorWindow)
        {
            this.editorWindow = editorWindow;
            graphViewChanged += OnGraphViewChanged;
            AddGridBackground();
            //AddToolWindow();
            //AddPreviewWindow();
            AddSearchWindow();
            AddStyles();
            AddManipulators();
            AddDefaultNodes();
        }

        private void AddSearchWindow()
        {
            if (_searchWindow == null)
            {
                _searchWindow = ScriptableObject.CreateInstance<GASearchWindow>();
                _searchWindow.Initialise(this);

            }
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        public GraphViewNode CreateNode(NodeType type, Vector2 position, Data.NodeSetting nodeSetting = null)
        {
            Type nodeType = Type.GetType($"SSL.Graph.Elements.{type}Node");
            GraphViewNode node = (GraphViewNode)Activator.CreateInstance(nodeType);
            node.GraphView = this;
            node.Initialise(position);

            if (nodeSetting == null)
                nodeSetting = new Data.NodeSetting();

            node.LoadSettings(nodeSetting);
            node.Draw();
            _nodes.Add(node);

            node.onSettingEdit += OnNodeChange;

            return node;
        }

        public string CutCopyNode(IEnumerable<GraphElement> elements)
        {
            string output = "";
            foreach (var item in elements)
            {
                GraphViewNode node = (GraphViewNode)item;
                output += "," + node.ID;
            }
            output.TrimStart(new char[1] { ',' });
            return output;
        }

        public void PasteNode(string operationName, string data)
        {
            var copiedNodeIDs = data.Split(new char[1] { ',' });
            if(copiedNodeIDs.Length > 0)
            {
                for (int i = 0; i < copiedNodeIDs.Length; i++)
                {
                    GraphViewNode nodeFromID = GAGenDataUtils.IDToGraphViewNode(copiedNodeIDs[i], Nodes);
                    if(nodeFromID != null)
                        AddElement(CopyNode(nodeFromID));
                }
            }
        }

        public GraphViewNode CopyNode(GraphViewNode originalNode)
        {
            NodeType type = originalNode.NodeType;
            Vector2 centre = editorWindow.position.center;
            Vector2 position = GetLocalMousePosition(centre, true);
            return CreateNode(type, position, originalNode.GetSettings());
        }

        void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            serializeGraphElements += CutCopyNode;
            unserializeAndPaste += PasteNode;
            canPasteSerializedData += x => true;
        }

        public void OnNodeChange()
        {

        }

        private IManipulator CreateNodeContextualManipulator(string actionTitle, NodeType type)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(type, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                    return;
                if (startPort.node == port.node)
                    return;
                if (startPort.direction == port.direction)
                    return;

                GAPortData startPortData = (GAPortData)startPort.userData;
                GAPortData portData = (GAPortData)port.userData;

                if (startPortData.PortType == portData.PortType)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        void AddDefaultNodes()
        {
            EditorApplication.delayCall += CentreGraphOnNodes;
        }

        public void CentreGraphOnNodes()
        {
            Vector2 averagePos = Vector2.zero;
            for (int i = 0; i < Nodes.Count; i++)
            {
                Rect pos = Nodes[i].GetPosition();
                averagePos += pos.center;
            }
            averagePos = averagePos / Nodes.Count;
            //Add dif between averagePos and centre to contentviewContainer
            Vector2 center = this.WorldToLocal(this.worldBound.center);
            Vector3 dif = center - averagePos;
            if (float.IsNaN(dif.x) || float.IsNaN(dif.y) || float.IsNaN(dif.z))
            {
                EditorApplication.delayCall += CentreGraphOnNodes;
            }
            else
            {
                contentViewContainer.transform.position = dif;
            }
        }

        void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddToolWindow()
        {
            Vector2 blackBoardPosition = new Vector2(0f, 0f);
            Blackboard blackboardWindow = new Blackboard(this)
            {
                title = "Variables"
            };
            Insert(1, blackboardWindow);
        }

        private void AddPreviewWindow()
        {
            GAPreview previewWindow = new GAPreview();
            previewWindow.Initialise(this);
            previewWindow.name = "previewWindow";
            Insert(1, previewWindow);
        }

        public GraphViewChange OnGraphViewChanged(GraphViewChange gvc)
        {
            if (gvc.elementsToRemove != null)
            {
                List<GraphViewNode> nodesToRemove = gvc.elementsToRemove.OfType<GraphViewNode>().ToList();
                foreach (GraphViewNode element in nodesToRemove)
                {
                    element.DisconnectAllPorts();
                    RemoveElement(element);
                    Nodes.Remove(element);
                }
                EditorApplication.delayCall += OnEdgesChange;
            }

            if(gvc.edgesToCreate != null)
            {
                EditorApplication.delayCall += OnEdgesChange;
            }

            return gvc;
        }

        public void OnEdgesChange()
        {
            List<Edge> orderedEdges = new List<Edge>();
            edges.ForEach(e => orderedEdges.Add(e));

        }

        void AddStyles()
        {
            StyleSheet styleSheet = (StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphViewStyle.uss", typeof(StyleSheet));
            styleSheets.Add(styleSheet);
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldPosition = mousePosition;

            if (isSearchWindow)
            {
                worldPosition -= editorWindow.position.position;
            }

            Vector2 localPosition = contentViewContainer.WorldToLocal(worldPosition);
            return localPosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));
            _nodes = new List<GraphViewNode>();
        }
    }
}