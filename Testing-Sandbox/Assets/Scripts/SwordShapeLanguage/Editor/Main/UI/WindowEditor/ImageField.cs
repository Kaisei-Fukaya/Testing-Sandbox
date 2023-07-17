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
    public class ImageField : VisualElement
    {
        VisualElement _picker;
        Button _pickerButton;
        TextElement _pickerTag;
        string _loadedPath = "";

        Image _image;

        public ImageField(string tagText)
        {
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetImageFieldStyle.uss", typeof(StyleSheet)));

            _picker = new VisualElement() 
            {
                name = "picker"
            };
            _pickerButton = new Button()
            {
                name = "pickerButton",
                text = "..."
            };
            _pickerTag = new TextElement()
            {
                name = "pickerTag",
                text = tagText
            };

            _picker.Add(_pickerTag);
            _picker.Add(_pickerButton);

            _image = new Image()
            {
                name = "image",
                scaleMode = ScaleMode.StretchToFill
            };

            _pickerButton.clicked += LoadImage;

            this.Add(_picker);
            this.Add(_image);
        }

        void LoadImage()
        {
            string filePath = EditorUtility.OpenFilePanel("Please provide an image", "", "");
            Texture2D loadedImage = Inference.LoadImage(filePath);
            _loadedPath = filePath;
            _image.image = loadedImage;
        }

        public Texture2D GetImage()
        {
            if (_image.image != null)
                return (Texture2D)_image.image;
            return null;
        }

        public string GetLoadedPath()
        {
            return _loadedPath;
        }
    }
}