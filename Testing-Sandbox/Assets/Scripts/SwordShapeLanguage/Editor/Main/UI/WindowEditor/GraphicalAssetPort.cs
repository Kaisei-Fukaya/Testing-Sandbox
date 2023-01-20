using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using SSL.Graph.Utils;
using System;

namespace SSL.Graph
{
    public class GraphicalAssetPort : VisualElement
    {
        GraphViewNode _parentNode;
        public GAPortType PortType { get { return _portType; } }
        GAPortType _portType;
        Orientation _orientation;
        Direction _direction;
        Port.Capacity _capacity;
        Port _trainPort, _genPort;
        Color _trainPortColor, _genPortColor, _trainPortDisabledColor, _genPortDisabledColor;
        string _customPortName;
        public string PortName { 
            get
            {
                if (_customPortName == string.Empty)
                {
                    return PortType.ToString();
                }
                return $"{_customPortName} ({PortType})";
            }
        }
        public GraphicalAssetPort(GraphViewNode parent, GAPortType portType, Orientation orientation, Direction direction, Port.Capacity capacity, string customPortName = "")
        {
            _parentNode = parent;
            _portType = portType;
            _orientation = orientation;
            _direction = direction;
            _capacity = capacity;
            _trainPortColor = new Color(67f / 255f, 165f / 255f, 196f / 255f);
            _trainPortDisabledColor = new Color(28f / 255f, 81f / 255f, 98f / 255f);
            _genPortColor = new Color(51f / 255f, 141f / 255f, 78f / 255f);
            _genPortDisabledColor = new Color(51f / 255f, 107f / 255f, 68f / 255f);
            _customPortName = customPortName;
            Draw();
            if(parent != null && parent.GraphView != null)
                parent.GraphView.editorWindow.onModeChange += SetMode;
        }

        public void Draw()
        {
            if (_parentNode == null || _parentNode.GraphView == null)
                return;
            VisualElement portContainer = new VisualElement();
            portContainer.AddToClassList("port-container");

            GAPortData trainPortData = new GAPortData()
            {
                Name = _customPortName,
                PortType = _portType,
                PortModeType = GAPortModeType.Train
            };

            GAPortData genPortData = new GAPortData()
            {
                Name = _customPortName,
                PortType = _portType,
                PortModeType = GAPortModeType.Generate
            };

            _trainPort = _parentNode.InstantiatePort(_orientation, _direction, _capacity, typeof(bool));
            _trainPort.userData = trainPortData;
            _trainPort.AddToClassList("train-port");

            _genPort = _parentNode.InstantiatePort(_orientation, _direction, _capacity, typeof(bool));
            _genPort.userData = genPortData;
            _genPort.AddToClassList("gen-port");

            _trainPort.style.paddingLeft = 0;
            _trainPort.style.paddingRight = 0;
            _genPort.style.paddingLeft = 0;
            _genPort.style.paddingRight = 0;

            Label portLabel = new Label()
            {
                text = trainPortData.Name
            };
            portLabel.AddToClassList("port-label");

            _trainPort.portName = trainPortData.Name;
            _genPort.portName = genPortData.Name;

            portContainer.Add(_trainPort);
            portContainer.Add(_genPort);


            //Different order depending on direction
            if (_direction == Direction.Input)
                contentContainer.Add(portContainer);

            contentContainer.Add(portLabel);

            if (_direction == Direction.Output)
                contentContainer.Add(portContainer);

            SetMode(_parentNode.GraphView.editorWindow.inTrainingMode);
        }

        public Port GetPort(bool trainPort)
        {
            if (trainPort)
                return _trainPort;
            return _genPort;
        }

        public void DisconnectAll()
        {
            if(_trainPort != null)
                _trainPort.DisconnectAll();
            if(_genPort != null)
                _genPort.DisconnectAll();
        }

        public IEnumerable<Edge> Connections(bool trainingPort)
        {

            if (trainingPort)
            {
                if (_trainPort == null)
                    return default;
                return _trainPort.connections;
            }
            if (_genPort == null)
                return default;
            return _genPort.connections;
        }

        public Edge GetConnectedEdge(bool trainingPort)
        {
            if (trainingPort)
            {
                if (_trainPort == null)
                    return null;
                return _trainPort.connections.SingleOrDefault();
            }
            if (_genPort == null)
                return null;
            return _genPort.connections.SingleOrDefault();
        }

        public Edge ConnectTo(Port other, bool inTrainingMode)
        {
            if (inTrainingMode)
                return _trainPort.ConnectTo(other);
            return _genPort.ConnectTo(other);
        }

        public void SetMode(bool inTrainingMode)
        {
            if (inTrainingMode)
            {
                _trainPort.portColor = _trainPortColor;
                _genPort.portColor = _genPortDisabledColor;
                _trainPort.SetEnabled(true);
                _genPort.SetEnabled(false);

                foreach (Edge edge in _trainPort.connections)
                {
                    edge.SetEnabledFull(true);
                }

                foreach (Edge edge in _genPort.connections)
                {
                    edge.SetEnabledFull(false);
                }
            }
            else
            {
                _genPort.portColor = _genPortColor;
                _trainPort.portColor = _trainPortDisabledColor;
                _genPort.SetEnabled(true);
                _trainPort.SetEnabled(false);

                foreach (Edge edge in _genPort.connections)
                {
                    edge.SetEnabledFull(true);
                }

                foreach (Edge edge in _trainPort.connections)
                {
                    edge.SetEnabledFull(false);
                }
            }

        }
    }
}