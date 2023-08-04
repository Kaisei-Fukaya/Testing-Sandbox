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
        string _tmpName; //Name of temporary file created after preprocessing
        string _tmpPath;

        Image _image;

        public ImageField(string tagText, string tmpName)
        {
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetImageFieldStyle.uss", typeof(StyleSheet)));

            _tmpName = tmpName + ".png";
            _tmpPath = Preprocess.tmp_path + "/" + _tmpName;

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

            _pickerButton.clicked += CommenceLoadImage;

            this.Add(_picker);
            this.Add(_image);
        }

        void CommenceLoadImage()
        {
            string filePath = EditorUtility.OpenFilePanel("Please provide an image", "", "");
            if (filePath == string.Empty)
                return;
            Preprocess.PreprocessImage(filePath, _tmpName);
            _loadedPath = filePath;
            EditorApplication.delayCall += FinishLoadingImage;
        }

        void FinishLoadingImage()
        {
            Texture2D loadedImage = Inference.LoadImage(_tmpPath);
            _image.image = loadedImage;
            //_image.im
        }

        public Texture2D GetImage()
        {
            if (_image.image != null)
                return (Texture2D)_image.image;
            return null;
        }

        public string GetLoadedPath()
        {
            //return _loadedPath;
            return _tmpPath;
        }
    }
}