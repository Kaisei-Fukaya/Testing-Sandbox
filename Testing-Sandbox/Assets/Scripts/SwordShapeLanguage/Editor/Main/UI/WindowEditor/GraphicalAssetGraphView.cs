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
        PreviewBox _previewWindow;
        public PreviewBox PreviewWindow { set { _previewWindow = value; } }
        bool _nodeUpdateFlag;
        public GraphicalAssetGraphView(GraphicalAssetGeneratorWindow editorWindow)
        {
            this.editorWindow = editorWindow;
            graphViewChanged += OnGraphViewChanged;
            //AddPreviewWindow();
            AddGridBackground();
            //AddToolWindow();
            AddSearchWindow();
            AddStyles();
            AddManipulators();
            AddDefaultNodes();
            EditorApplication.update += EditorStateUpdate;
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
            NodeUpdateFlag();
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
            _previewWindow.UpdateMesh(GAGenDataUtils.NodesToData(Nodes));
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
            _previewWindow = new PreviewBox();
            _previewWindow.Initialise(this, GAGenDataUtils.NodesToData(Nodes));
            _previewWindow.StretchToParentSize();
            _previewWindow.name = "previewWindow";
            Insert(0, _previewWindow);
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
                OnEdgesChange(removedEdges: gvc.elementsToRemove.OfType<Edge>().ToList());
            }

            if(gvc.edgesToCreate != null)
            {
                OnEdgesChange(addedEdges: new List<Edge>(gvc.edgesToCreate));
            }

            EditorApplication.delayCall += OnNodeChange;

            return gvc;
        }

        public void OnEdgesChange(List<Edge> addedEdges = null, List<Edge> removedEdges = null)
        {
            if (addedEdges != null)
            {
                foreach (var edge in addedEdges)
                {
                    var outputNode = (GraphViewNode)edge.input.node;
                    var inputNode = (GraphViewNode)edge.output.node;

                    //Find the index of output in input
                    var outgoingList = inputNode.OutgoingPorts;
                    var outputNodeConnectionData = outgoingList.Where(x => x.GetPort() == edge.output);
                    var indexOfOutputInInput = outgoingList.IndexOf(outputNodeConnectionData.First());

                    Orientation orientation = EvaluateOrientation(inputNode.WorldOrientation, indexOfOutputInInput);
                    AssignChildOrientation(outputNode, orientation);
                }
            }

            if (removedEdges != null)
            {
                foreach (var edge in removedEdges)
                {
                    var outputNode = (GraphViewNode)edge.input.node;
                    if (outputNode == null)
                        continue;
                    AssignChildOrientation(outputNode, 0);
                }
            }

            void AssignChildOrientation(GraphViewNode currentNode, Orientation orientation)
            {
                currentNode.WorldOrientation = orientation;
                var outGoingConnections = currentNode.GetOutgoingConnectionIDs();

                if (outGoingConnections.Count > 5)
                    throw new System.Exception("The number of outgoing connection exceed the orientation limit of 5");

                for (int i = 0; i < outGoingConnections.Count; i++)
                {
                    var child = GAGenDataUtils.IDToGraphViewNode(outGoingConnections[i].iD, Nodes);
                    if (child != null)
                    {
                        AssignChildOrientation(child, EvaluateOrientation(orientation, i));
                    }
                }
            }

        }

        public void NodeUpdateFlag()
        {
            _nodeUpdateFlag = true;
        }

        public void EditorStateUpdate()
        {
            if (_nodeUpdateFlag)
            {
                _nodeUpdateFlag = false;
                OnNodeChange();
            }
        }
        public static Orientation EvaluateOrientation(Orientation previousWorldOrientation, /*Orientation forwardOrientation,*/ int faceIndex)
        {
            switch (previousWorldOrientation)
            {
                case Orientation.Up:
                    return previousWorldOrientation + faceIndex;
                case Orientation.Left:
                    return EvalLeft();
                case Orientation.Forward:
                    return EvalForward();
                case Orientation.Right:
                    return EvalRight();
                case Orientation.Backward:
                    return EvalBackward();
                case Orientation.Down:
                    return EvalDown();
            }

            return Orientation.Up;

            Orientation EvalLeft()
            {
                switch (faceIndex)
                {
                    default:
                        return Orientation.Left;
                    case 1:
                        return Orientation.Backward;
                    case 2:
                        return Orientation.Down;
                    case 3:
                        return Orientation.Forward;
                    case 4:
                        return Orientation.Up;
                    case 5:
                        return Orientation.Right;
                }
            }

            Orientation EvalForward()
            {
                switch (faceIndex)
                {
                    default:
                        return Orientation.Forward;
                    case 1:
                        return Orientation.Left;
                    case 2:
                        return Orientation.Down;
                    case 3:
                        return Orientation.Right;
                    case 4:
                        return Orientation.Up;
                    case 5:
                        return Orientation.Backward;
                }
            }

            Orientation EvalRight()
            {
                switch (faceIndex)
                {
                    default:
                        return Orientation.Right;
                    case 1:
                        return Orientation.Forward;
                    case 2:
                        return Orientation.Down;
                    case 3:
                        return Orientation.Backward;
                    case 4:
                        return Orientation.Up;
                    case 5:
                        return Orientation.Left;
                }
            }

            Orientation EvalBackward()
            {
                switch (faceIndex)
                {
                    default:
                        return Orientation.Backward;
                    case 1:
                        return Orientation.Right;
                    case 2:
                        return Orientation.Down;
                    case 3:
                        return Orientation.Left;
                    case 4:
                        return Orientation.Up;
                    case 5:
                        return Orientation.Forward;
                }
            }

            Orientation EvalDown()
            {
                switch (faceIndex)
                {
                    default:
                        return Orientation.Down;
                    case 1:
                        return Orientation.Left;
                    case 2:
                        return Orientation.Forward;
                    case 3:
                        return Orientation.Right;
                    case 4:
                        return Orientation.Backward;
                    case 5:
                        return Orientation.Up;
                }
            }

            //Orientation ShiftForward(Orientation newOrientation)
            //{
            //    switch (forwardOrientation)
            //    {
            //        case Orientation.Up:
            //            return newOrientation;
            //        case Orientation.Left:
            //            return Shift(newOrientation, 1);
            //        case Orientation.Forward:
            //            return Shift(newOrientation, 1);
            //        case Orientation.Right:
            //            return Shift(newOrientation, 1);
            //        case Orientation.Backward:
            //            return Shift(newOrientation, 1);
            //        case Orientation.Down:
            //            return Shift(newOrientation, 2);
            //    }
            //    return newOrientation;
            //}

            Orientation Shift(Orientation orientation, int amount)
            {
                int value = (int)orientation + amount;
                if (value > 5)
                {
                    value = value - 6;
                }
                return (Orientation)value;
            }
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