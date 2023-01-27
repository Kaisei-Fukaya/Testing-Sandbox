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
        protected IntegerField _nLoopsField, _subMeshIndexField;
        protected FloatField _roundingField;
        protected Slider _relativeForwardTaperField, _relativeBackwardTaperField;
        protected Vector3Field _sizeField;

        public override void Initialise(Vector2 position)
        {
            NodeType = NodeType.Branch;
            base.Initialise(position);
            _nLoopsField = new IntegerField();
            _subMeshIndexField = new IntegerField();
            _roundingField = new FloatField();
            _relativeForwardTaperField = new Slider(0f, 1f);
            _relativeBackwardTaperField = new Slider(0f, 1f);
            _sizeField = new Vector3Field();
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting setting = base.GetSettings();
            setting.parameters.nLoops = _nLoopsField.value;
            setting.parameters.size = _sizeField.value;
            setting.parameters.relativeForwardTaper = _relativeForwardTaperField.value;
            setting.parameters.relativeBackwardTaper = _relativeBackwardTaperField.value;
            setting.parameters.rounding = _roundingField.value;
            setting.parameters.subMeshIndex = _subMeshIndexField.value;
            return setting;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            _nLoopsField.SetValueWithoutNotify(setting.parameters.nLoops);
            _sizeField.SetValueWithoutNotify(setting.parameters.size);
            _relativeForwardTaperField.SetValueWithoutNotify(setting.parameters.relativeForwardTaper);
            _relativeBackwardTaperField.SetValueWithoutNotify(setting.parameters.relativeBackwardTaper);
            _roundingField.SetValueWithoutNotify(setting.parameters.rounding);
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

            VisualElement nLoopsBlock = new VisualElement();
            nLoopsBlock.AddToClassList("label-field-block");
            nLoopsBlock.Add(new Label("nLoops:"));
            nLoopsBlock.Add(_nLoopsField);

            VisualElement sizeBlock = new VisualElement();
            sizeBlock.AddToClassList("label-field-block");
            sizeBlock.Add(new Label("Size:"));
            sizeBlock.Add(_sizeField);

            VisualElement roundingBlock = new VisualElement();
            roundingBlock.AddToClassList("label-field-block");
            roundingBlock.Add(new Label("Rounding:"));
            roundingBlock.Add(_roundingField);

            VisualElement forwardTaperBlock = new VisualElement();
            forwardTaperBlock.AddToClassList("label-field-block");
            forwardTaperBlock.Add(new Label("Taper:"));
            forwardTaperBlock.Add(_relativeForwardTaperField);

            VisualElement backwardTaperBlock = new VisualElement();
            backwardTaperBlock.AddToClassList("label-field-block");
            backwardTaperBlock.Add(new Label("Taper:"));
            backwardTaperBlock.Add(_relativeBackwardTaperField);

            VisualElement sMeshIndexBlock = new VisualElement();
            sMeshIndexBlock.AddToClassList("label-field-block");
            sMeshIndexBlock.Add(new Label("Submesh Index:"));
            sMeshIndexBlock.Add(_subMeshIndexField);



            optionsBlock.Add(nLoopsBlock);
            optionsBlock.Add(sizeBlock);
            optionsBlock.Add(roundingBlock);
            optionsBlock.Add(forwardTaperBlock);
            optionsBlock.Add(backwardTaperBlock);
            optionsBlock.Add(sMeshIndexBlock);
            return optionsBlock;
        }

    }
}