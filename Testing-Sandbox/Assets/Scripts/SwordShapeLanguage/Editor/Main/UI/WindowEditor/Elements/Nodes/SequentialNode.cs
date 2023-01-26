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
    public class SequentialNode : GraphViewNode
    {
        protected GraphicalAssetPort _inputPort, _outputPort;
        protected GAPortType _inputPortType, _outputPortType;
        protected IntegerField _nLoopsField, _subMeshIndexField;
        protected FloatField _roundingField;
        protected Slider _relativeForwardTaperField, _relativeBackwardTaperField;
        protected Vector3Field _sizeField;

        public override void Initialise(Vector2 position)
        {
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

            _inputPort = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            _ingoingPorts.Add(_inputPort);
            _outgoingPorts.Add(_outputPort);
            inputContainer.Add(_inputPort);
            outputContainer.Add(_outputPort);

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