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
    public class SegmentNode : GraphViewNode
    {
        protected GraphicalAssetPort _inputPort, _outputPort;
        protected GAPortType _inputPortType, _outputPortType;
        protected IntegerField _nLoopsField, _subMeshIndexField;
        protected FloatField _roundingField;
        protected Slider _relativeForwardTaperField, _relativeBackwardTaperField;
        protected FloatField _sizeWidthField, _sizeHeightField, _sizeDepthField;
        protected Foldout _deformFoldout, _curveFoldout;
        protected Vector3Field[] _deformFields;
        protected Vector2Field _tipOffsetField;
        protected Vector2Field _curveControlField;
        protected bool _curveNotAffectNormal;

        public override void Initialise(Vector2 position)
        {
            NodeType = NodeType.Segment;
            base.Initialise(position);

            //Initialise fields
            _nLoopsField = new IntegerField();
            _subMeshIndexField = new IntegerField();
            _roundingField = new FloatField() { label = "Rounding:" };
            _relativeForwardTaperField = new Slider(0f, 1f);
            _relativeBackwardTaperField = new Slider(0f, 1f);
            _sizeWidthField = new FloatField()  { label = "Width:" }; 
            _sizeHeightField = new FloatField() { label = "Height:" };
            _sizeDepthField = new FloatField()  { label = "Depth:" };
            _deformFoldout = new Foldout() { text = "Deforms" };
            _deformFields = new Vector3Field[0];
            _curveFoldout = new Foldout() { text = "Curve" };
            _curveFoldout.AddToClassList("curve-foldout");
            _tipOffsetField = new Vector2Field("Tip offset");
            _curveControlField = new Vector2Field("Curve");

            //Add callbacks
            _nLoopsField.RegisterValueChangedCallback(x => CallSettingsEditEvent());
            _subMeshIndexField.RegisterValueChangedCallback(x => CallSettingsEditEvent());
            _roundingField.RegisterValueChangedCallback(x => CallSettingsEditEvent());
            _relativeForwardTaperField.RegisterValueChangedCallback(x => CallSettingsEditEvent());
            _relativeBackwardTaperField.RegisterValueChangedCallback(x => CallSettingsEditEvent());
            _sizeWidthField.RegisterValueChangedCallback(x => CallSettingsEditEvent());
            _sizeHeightField.RegisterValueChangedCallback(x => CallSettingsEditEvent());
            _sizeDepthField.RegisterValueChangedCallback(x => CallSettingsEditEvent());
            _tipOffsetField.RegisterValueChangedCallback(x => CallSettingsEditEvent());
            _curveControlField.RegisterValueChangedCallback(x => CallSettingsEditEvent());

            //IO
            _ingoingPorts = new List<GraphicalAssetPort>();
            _outgoingPorts = new List<GraphicalAssetPort>();

            _inputPort = new GraphicalAssetPort(this, _inputPortType, UnityEditor.Experimental.GraphView.Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            _outputPort = new GraphicalAssetPort(this, _outputPortType, UnityEditor.Experimental.GraphView.Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            _ingoingPorts.Add(_inputPort);
            _outgoingPorts.Add(_outputPort);

        }

        public override NodeSetting GetSettings()
        {
            NodeSetting setting = base.GetSettings();
            setting.parameters.nLoops = _nLoopsField.value;
            setting.parameters.size = new Vector3(_sizeWidthField.value, _sizeHeightField.value, _sizeDepthField.value);
            setting.parameters.relativeForwardTaper = _relativeForwardTaperField.value;
            setting.parameters.relativeBackwardTaper = _relativeBackwardTaperField.value;
            setting.parameters.rounding = _roundingField.value;
            setting.parameters.subMeshIndex = _subMeshIndexField.value;
            setting.parameters.curveParams.tipOffset = _tipOffsetField.value;
            setting.parameters.curveParams.controlPoint = _curveControlField.value;
            setting.parameters.curveNotAffectNormal = _curveNotAffectNormal;
            for (int i = 0; i < setting.parameters.deforms.Length; i++)
            {
                setting.parameters.deforms[i] = _deformFields[i].value;
            }
            return setting;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            _nLoopsField.SetValueWithoutNotify(setting.parameters.nLoops);
            _sizeWidthField.SetValueWithoutNotify(setting.parameters.size.x);
            _sizeHeightField.SetValueWithoutNotify(setting.parameters.size.y);
            _sizeDepthField.SetValueWithoutNotify(setting.parameters.size.z);
            _relativeForwardTaperField.SetValueWithoutNotify(setting.parameters.relativeForwardTaper);
            _relativeBackwardTaperField.SetValueWithoutNotify(setting.parameters.relativeBackwardTaper);
            _roundingField.SetValueWithoutNotify(setting.parameters.rounding);
            _subMeshIndexField.SetValueWithoutNotify(setting.parameters.subMeshIndex);
            _deformFields = new Vector3Field[setting.parameters.deforms.Length];
            _tipOffsetField.SetValueWithoutNotify(setting.parameters.curveParams.tipOffset);
            _curveControlField.SetValueWithoutNotify(setting.parameters.curveParams.controlPoint);
            _curveFoldout.Add(_tipOffsetField);
            _curveFoldout.Add(_curveControlField);
            for (int i = 0; i < _deformFields.Length; i++)
            {
                _deformFields[i] = new Vector3Field();
                _deformFields[i].SetValueWithoutNotify(setting.parameters.deforms[i]);
                _deformFields[i].RegisterValueChangedCallback(x => CallSettingsEditEvent());
                _deformFoldout.Add(_deformFields[i]);
            }

            _curveNotAffectNormal = setting.parameters.curveNotAffectNormal;
        }

        public override void Draw()
        {
            base.Draw();

            ConfigPort(_inputPort, 5);
            ConfigPort(_outputPort, 0);

            inputContainer.Add(_inputPort);
            outputContainer.Add(_outputPort);

            extensionContainer.Add(CreateOptions());

            RefreshExpandedState();
        }

        protected VisualElement CreateOptions()
        {
            VisualElement optionsBlock = new VisualElement();

            VisualElement shapeGroup = new VisualElement();
            shapeGroup.AddToClassList("group");
            TextElement shapeHeader = new TextElement() 
            {
                text = "Shape"
            };
            shapeHeader.AddToClassList("header");

            VisualElement sizeBlock = new VisualElement();
            sizeBlock.AddToClassList("label-field-block");
            sizeBlock.Add(_sizeWidthField);
            sizeBlock.Add(_sizeHeightField);
            sizeBlock.Add(_sizeDepthField);



            VisualElement roundingBlock = new VisualElement();
            roundingBlock.AddToClassList("label-field-block");
            roundingBlock.Add(_roundingField);

            VisualElement forwardTaperBlock = new VisualElement();
            forwardTaperBlock.AddToClassList("slider-group");
            forwardTaperBlock.Add(new Label("Taper:"));
            forwardTaperBlock.Add(_relativeForwardTaperField);

            VisualElement backwardTaperBlock = new VisualElement();
            backwardTaperBlock.AddToClassList("slider-group");
            backwardTaperBlock.Add(new Label("Taper:"));
            backwardTaperBlock.Add(_relativeBackwardTaperField);
            
            VisualElement roundingAndTaperBlock = new VisualElement();
            roundingAndTaperBlock.Add(roundingBlock);
            roundingAndTaperBlock.Add(forwardTaperBlock);
            roundingAndTaperBlock.Add(backwardTaperBlock);

            VisualElement shapeContentContainer = new VisualElement() { name = "shapeBlockContent" };
            shapeContentContainer.Add(sizeBlock);
            shapeContentContainer.Add(roundingAndTaperBlock);

            shapeGroup.Add(shapeHeader);
            shapeGroup.Add(shapeContentContainer);

            VisualElement meshDetailsGroup = new VisualElement();
            meshDetailsGroup.AddToClassList("group");
            TextElement meshDetailsHeader = new TextElement()
            {
                text = "Mesh Details"
            };
            meshDetailsHeader.AddToClassList("header");

            VisualElement nLoopsBlock = new VisualElement();
            nLoopsBlock.AddToClassList("label-field-block");
            nLoopsBlock.Add(new Label("Loop count:"));
            nLoopsBlock.Add(_nLoopsField);

            VisualElement sMeshIndexBlock = new VisualElement();
            sMeshIndexBlock.AddToClassList("label-field-block");
            sMeshIndexBlock.Add(new Label("Submesh Index:"));
            sMeshIndexBlock.Add(_subMeshIndexField);

            meshDetailsGroup.Add(meshDetailsHeader);
            meshDetailsGroup.Add(nLoopsBlock);
            meshDetailsGroup.Add(sMeshIndexBlock);


            optionsBlock.Add(shapeGroup);
            optionsBlock.Add(meshDetailsGroup);
            optionsBlock.Add(_curveFoldout);
            optionsBlock.Add(_deformFoldout);
            _deformFoldout.value = false;
            return optionsBlock;
        }

    }
}