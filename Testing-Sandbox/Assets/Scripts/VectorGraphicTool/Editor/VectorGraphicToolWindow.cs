using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using SSL.Data;
using SSL.Data.Utils;
using System.Xml.Serialization;


public class VectorGraphicToolWindow : EditorWindow
{
    StyleSheet style;
    VisualElement _mainView;

    [MenuItem("Window/Vector Editor")]
    public static void ShowWindow()
    {
        GetWindow<VectorGraphicToolWindow>();
    }

    private void CreateGUI()
    {
        this.titleContent = new GUIContent("Vector Editor");
        style = (StyleSheet)AssetDatabase.LoadAssetAtPath($"Assets/Scripts/VectorGraphicTool/Editor/VectorToolStyle.uss", typeof(StyleSheet));
        AddToolbar();
        _mainView = new VisualElement()
        {
            name = "mainView"
        };
        _mainView.AddToClassList("root");
        //mainView.StretchToParentSize();
        rootVisualElement.Add(_mainView);
        rootVisualElement.styleSheets.Add(style);
        AddCanvas();
    }

    private void AddToolbar()
    {
        Toolbar toolbar = new Toolbar();

        ToolbarButton saveButton = new ToolbarButton()
        {
            text = "Save"
        };
        saveButton.clicked += Save;

        ToolbarButton saveAsButton = new ToolbarButton()
        {
            text = "Save As"
        };
        saveAsButton.clicked += SaveAs;

        ToolbarButton loadButton = new ToolbarButton()
        {
            text = "Load"
        };
        loadButton.clicked += Load;

        ToolbarSpacer spacer1 = new ToolbarSpacer();
        ToolbarSpacer spacer2 = new ToolbarSpacer();
        ToolbarSpacer spacer3 = new ToolbarSpacer();
        toolbar.Add(saveButton);
        toolbar.Add(spacer1);
        toolbar.Add(saveAsButton);
        toolbar.Add(loadButton);
        toolbar.Add(spacer2);
        toolbar.Add(spacer3);

        rootVisualElement.Add(toolbar);
    }
    private void AddCanvas()
    {
        VisualElement canvas = new VisualElement();
        canvas.AddToClassList("canvas");
        VectorGraphic vg = new VectorGraphic();
        canvas.Add(vg);
        _mainView.Add(canvas);
        _mainView.AddManipulator(new CanvasPan());
    }

    void Save()
    {

    }

    void SaveAs()
    {

    }

    void Load()
    {
        
    }
}
