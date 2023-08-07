using System;
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
    public class ConfigBox : VisualElement
    {
        VisualElement _mainContainer;
        int _currentSelectedTab = 0;
        List<VisualElement> _tabs = new List<VisualElement>();
        List<TextElement> _tabButtons = new List<TextElement>();
        public Inference inference;
        Action<GAGenData> _loadMethod;
        Action<int> _setSubdivMethod;
        Action<bool> _setShadingMethod;
        Action<float> _setSpacingMethod;
        Action<List<(int, Material)>> _setMaterialsMethod;

        private MaterialsPopupWindow _matPopupWindow;

        GraphicalAssetGeneratorWindow _parentWindow;

        public void Initialise(GraphicalAssetGeneratorWindow parentWindow,
            Action<GAGenData> loadMethod, 
            Action<int> setSubdivMethod, 
            Action<float> setSpacingMethod, 
            Action<bool> setShadingMethod, 
            Action<List<(int, Material)>> setMaterialsMethod)
        {
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetConfigStyle.uss", typeof(StyleSheet)));
            _loadMethod = loadMethod;
            _setSubdivMethod = setSubdivMethod;
            _setSpacingMethod = setSpacingMethod;
            _setShadingMethod = setShadingMethod;
            _setMaterialsMethod = setMaterialsMethod;

            _parentWindow = parentWindow;

            VisualElement topContainer = new VisualElement()
            {
                name = "topContainer"
            };
            _mainContainer = new VisualElement()
            {
                name = "mainContainer"
            };

            TextElement titleElement = new TextElement()
            {
                text = "Config",
                name = "titleElement"
            };

            topContainer.Add(titleElement);

            contentContainer.Add(topContainer);
            contentContainer.Add(_mainContainer);
            PopulateTabs();
            AddParamSection();
        }

        void AddParamSection()
        {
            VisualElement paramBox = new VisualElement()
            {
                name = "paramBox"
            };

            VisualElement resolutionBlock = new VisualElement();
            resolutionBlock.AddToClassList("inputBlock");
            SliderInt resolutionSlider = new SliderInt(1, 5);
            resolutionSlider.label = "Subdivisions:";
            resolutionBlock.Add(resolutionSlider);

            VisualElement facetedShadingBlock = new VisualElement();
            facetedShadingBlock.AddToClassList("inputBlock");
            Toggle facetedShadingToggle = new Toggle();
            facetedShadingToggle.label = "Faceted Shading:";
            facetedShadingBlock.Add(facetedShadingToggle);


            VisualElement spacingBlock = new VisualElement();
            spacingBlock.AddToClassList("inputBlock");
            FloatField spacingField = new FloatField() { label = "Segment Gap:" };
            spacingBlock.Add(spacingField);

            Button materialsButton = new Button();
            materialsButton.text = "Assign Materials";
            materialsButton.AddToClassList("inputBlock");
            materialsButton.clicked += () => OpenMaterialsWindow(materialsButton);

            resolutionSlider.SetValueWithoutNotify(2);
            resolutionSlider.RegisterValueChangedCallback(x => _setSubdivMethod(x.newValue));
            spacingField.RegisterValueChangedCallback(x => { float v = Mathf.Max(0f, x.newValue); spacingField.SetValueWithoutNotify(v); _setSpacingMethod(v); });
            facetedShadingToggle.SetValueWithoutNotify(false);
            facetedShadingToggle.RegisterValueChangedCallback(x => _setShadingMethod(x.newValue));

            paramBox.Add(resolutionBlock);
            paramBox.Add(facetedShadingBlock);
            //paramBox.Add(spacingBlock);
            paramBox.Add(materialsButton);

            _mainContainer.Insert(0, paramBox);
        }


        void OpenMaterialsWindow(Button button)
        {
            if (_matPopupWindow == null)
            {
                _matPopupWindow = EditorWindow.CreateInstance<MaterialsPopupWindow>();
                _matPopupWindow.ShowUtility();
                _matPopupWindow.Initialise(_parentWindow.materials, _setMaterialsMethod);

                return;
            }
            _matPopupWindow.Close();
        }

        public void CloseMaterialsWindow()
        {
            if (_matPopupWindow != null)
                _matPopupWindow.Close();
        }

        void PopulateTabs()
        {
            VisualElement tabButtonRow = new VisualElement()
            {
                name = "tabButtonRow"
            };

            TextElement randomTabButton = new TextElement()
            {
                name = "randomTabButton",
                text = "Random"
            };

            TextElement im2TabButton = new TextElement()
            {
                name = "im2TabButton",
                text = "Image Reconstruction"
            };

            TextElement interpTabButton = new TextElement()
            {
                name = "interpTabButton",
                text = "Interpolate"
            };

            randomTabButton.RegisterCallback<PointerDownEvent>(e =>
            {
                SelectAndLoadTab(0);
            });

            im2TabButton.RegisterCallback<PointerDownEvent>(e =>
            {
                SelectAndLoadTab(1);
            });

            interpTabButton.RegisterCallback<PointerDownEvent>(e =>
            {
                SelectAndLoadTab(2);
            });


            randomTabButton.AddToClassList("tabButton");
            im2TabButton.AddToClassList("tabButton");
            interpTabButton.AddToClassList("tabButton");
            _tabButtons.Add(randomTabButton);
            _tabButtons.Add(im2TabButton);
            _tabButtons.Add(interpTabButton);
            tabButtonRow.Add(randomTabButton);
            tabButtonRow.Add(im2TabButton);
            tabButtonRow.Add(interpTabButton);

            _tabs.Add(CreateRandomTab());
            _tabs.Add(CreateIm2Tab());
            _tabs.Add(CreateInterpTab());


            _mainContainer.Insert(0, tabButtonRow);

            SelectAndLoadTab(10); //Deliberately pick out of bounds index so that nothing is selected to begin with.
        }

        VisualElement CreateRandomTab()
        {
            VisualElement tab = new VisualElement()
            {
                name = "randomTab"
            };

            VisualElement tabSpacer = new VisualElement();
            tabSpacer.AddToClassList("tabSpacer");

            Button generateButton = new Button()
            {
                name = "randomGenerateButton",
                text = "Generate"
            };

            generateButton.clicked += () => {
                GAGenData result = inference.Rand2Model();
                if (result != null)
                {
                    _loadMethod(result);
                }
            };

            tab.Add(tabSpacer);
            tab.Add(generateButton);
            tab.AddToClassList("tab");
            _mainContainer.Add(tab);

            return tab;
        }

        VisualElement CreateIm2Tab()
        {
            VisualElement tab = new VisualElement()
            {
                name = "im2Tab"
            };

            VisualElement tabSpacer = new VisualElement();
            tabSpacer.AddToClassList("tabSpacer");

            Button generateButton = new Button()
            {
                name = "im2GenerateButton",
                text = "Generate"
            };

            ImageField imageField = new ImageField("Image", "rec")
            {
                name = "imageField"
            };

            generateButton.clicked += () => {
                GAGenData result = inference.Img2Model(imageField.GetLoadedPath());
                if (result != null)
                {
                    _loadMethod(result);
                }
            };

            tab.Add(tabSpacer);
            tab.Add(imageField);
            tab.Add(generateButton);
            tab.AddToClassList("tab");
            _mainContainer.Add(tab);

            return tab;
        }

        VisualElement CreateInterpTab()
        {
            VisualElement tab = new VisualElement()
            {
                name = "interpTab"
            };

            VisualElement tabSpacer = new VisualElement();
            tabSpacer.AddToClassList("tabSpacer");

            Button generateButton = new Button()
            {
                name = "interpGenerateButton",
                text = "Generate"
            };

            Slider tSlider = new Slider()
            {
                name = "tSlider",
                tooltip = "Set the 't' value of the interpolation, 0 = image A, 1 = image B",
                lowValue = 0f,
                highValue = 1f,
                value = 0.5f
            };

            FloatField tField = new FloatField()
            {
                name = "tField",
                label = "Interpolation",
                value = 0.5f
            };

            tField.RegisterValueChangedCallback(x => {
                float v = Mathf.Clamp(x.newValue, 0f, 1f);
                tField.SetValueWithoutNotify(v);
                tSlider.SetValueWithoutNotify(v);
            });
            tSlider.RegisterValueChangedCallback(x => tField.SetValueWithoutNotify(x.newValue));

            ImageField imageAField = new ImageField("First image", "intA")
            {
                name = "imageAField"
            };

            ImageField imageBField = new ImageField("Second image", "intB")
            {
                name = "imageBField"
            };

            VisualElement imageFieldGroup = new VisualElement()
            {
                name = "imageFieldGroup"
            };

            generateButton.clicked += () => {
                GAGenData interpResult = inference.Interp(imageAField.GetLoadedPath(), imageBField.GetLoadedPath(), tSlider.value);
                if (interpResult != null)
                {
                    _loadMethod(interpResult);
                }
            };

            imageFieldGroup.Add(imageAField);
            imageFieldGroup.Add(imageBField);

            tab.Add(tabSpacer);
            tab.Add(imageFieldGroup);
            tab.Add(tSlider);
            tab.Add(tField);
            tab.Add(generateButton);
            tab.AddToClassList("tab");
            _mainContainer.Add(tab);

            return tab;
        }

        void SelectAndLoadTab(int tab)
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                if(i == tab)
                {
                    _tabs[i].RemoveFromClassList("hiddenTab");
                }
                else
                {
                    _tabs[i].AddToClassList("hiddenTab");
                }
            }

            for (int i = 0; i < _tabButtons.Count; i++)
            {
                if (i == tab)
                {
                    _tabButtons[i].AddToClassList("tabFocused");
                }
                else
                {
                    _tabButtons[i].RemoveFromClassList("tabFocused");
                }
            }
        }
    }
}