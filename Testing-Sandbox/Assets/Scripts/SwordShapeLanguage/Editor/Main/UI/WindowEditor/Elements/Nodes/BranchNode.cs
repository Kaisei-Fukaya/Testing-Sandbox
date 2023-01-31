using SSL.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SSL.Graph.Elements
{
    public class BranchNode : GraphViewNode
    {
        protected GraphicalAssetPort _inputPortBottom, _outputPortTop, _outputPortLeft, _outputPortForward, _outputPortRight, _outputPortBackward;
        protected GAPortType _inputPortType, _outputPortType;
        protected IntegerField _subMeshIndexField;
        protected Vector3Field _sizeField;

        public override void Initialise(Vector2 position)
        {
            NodeType = NodeType.Branch;
            base.Initialise(position);
            _subMeshIndexField = new IntegerField();
            _sizeField = new Vector3Field();
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting setting = base.GetSettings();
            setting.parameters.size = _sizeField.value;
            setting.parameters.subMeshIndex = _subMeshIndexField.value;
            return setting;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            _sizeField.SetValueWithoutNotify(setting.parameters.size);
            _subMeshIndexField.SetValueWithoutNotify(setting.parameters.subMeshIndex);
        }

        public override void Draw()
        {
            base.Draw();
            _ingoingPorts = new List<GraphicalAssetPort>();
            _outgoingPorts = new List<GraphicalAssetPort>();

            _inputPortBottom = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            _outputPortTop = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            _outputPortLeft = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            _outputPortForward = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            _outputPortRight = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            _outputPortBackward = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            _ingoingPorts.Add(_inputPortBottom);
            _outgoingPorts.Add(_outputPortTop);
            _outgoingPorts.Add(_outputPortLeft);
            _outgoingPorts.Add(_outputPortForward);
            _outgoingPorts.Add(_outputPortRight);
            _outgoingPorts.Add(_outputPortBackward);
            inputContainer.Add(_inputPortBottom);
            outputContainer.Add(_outputPortTop);
            outputContainer.Add(_outputPortLeft);
            outputContainer.Add(_outputPortForward);
            outputContainer.Add(_outputPortRight);
            outputContainer.Add(_outputPortBackward);

            extensionContainer.Add(CreateOptions());

            RefreshExpandedState();
        }

        protected VisualElement CreateOptions()
        {
            VisualElement optionsBlock = new VisualElement();

            VisualElement sizeBlock = new VisualElement();
            sizeBlock.AddToClassList("label-field-block");
            sizeBlock.Add(new Label("Size:"));
            sizeBlock.Add(_sizeField);

            VisualElement sMeshIndexBlock = new VisualElement();
            sMeshIndexBlock.AddToClassList("label-field-block");
            sMeshIndexBlock.Add(new Label("Submesh Index:"));
            sMeshIndexBlock.Add(_subMeshIndexField);



            optionsBlock.Add(sizeBlock);
            optionsBlock.Add(sMeshIndexBlock);
            return optionsBlock;
        }

    }
}