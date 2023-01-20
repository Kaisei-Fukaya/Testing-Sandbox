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
        protected FloatField _roundingField, _relativeTaperField;
        protected Vector3Field _sizeField;

        public override void Initialise(Vector2 position)
        {
            base.Initialise(position);
            _nLoopsField = new IntegerField();
            _subMeshIndexField = new IntegerField();
            _roundingField = new FloatField();
            _relativeTaperField = new FloatField();
            _sizeField = new Vector3Field();
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting setting = base.GetSettings();
            setting.parameters.nLoops = _nLoopsField.value;
            setting.parameters.size = _sizeField.value;
            setting.parameters.relativeTaper = _relativeTaperField.value;
            setting.parameters.rounding = _roundingField.value;
            setting.parameters.subMeshIndex = _subMeshIndexField.value;
            return setting;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            _nLoopsField.SetValueWithoutNotify(setting.parameters.nLoops);
            _sizeField.SetValueWithoutNotify(setting.parameters.size);
            _relativeTaperField.SetValueWithoutNotify(setting.parameters.relativeTaper);
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

            VisualElement taperBlock = new VisualElement();
            taperBlock.AddToClassList("label-field-block");
            taperBlock.Add(new Label("Taper:"));
            taperBlock.Add(_relativeTaperField);

            VisualElement sMeshIndexBlock = new VisualElement();
            sMeshIndexBlock.AddToClassList("label-field-block");
            sMeshIndexBlock.Add(new Label("Submesh Index:"));
            sMeshIndexBlock.Add(_subMeshIndexField);



            optionsBlock.Add(nLoopsBlock);
            optionsBlock.Add(sizeBlock);
            optionsBlock.Add(roundingBlock);
            optionsBlock.Add(taperBlock);
            optionsBlock.Add(sMeshIndexBlock);
            return optionsBlock;
        }

    }
}