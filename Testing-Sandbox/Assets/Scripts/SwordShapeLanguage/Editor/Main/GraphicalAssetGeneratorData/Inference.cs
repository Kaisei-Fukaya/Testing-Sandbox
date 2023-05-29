using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEditor;
using SSL.Data.Utils;
using System.IO;
using System;

namespace SSL.Data
{
    public class Inference
    {
        NNModel _modelAsset;
        Model _model;
        IWorker _worker;

        public Inference()
        {
            _modelAsset = (NNModel)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/ONNX/model.onnx", typeof(NNModel));
            _model = ModelLoader.Load(_modelAsset);
            _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _model);
        }

        //Image to Model
        public GAGenData Img2Model(string path)
        {
            Texture2D image = LoadImage(path);
            Tensor input = new Tensor(image);
            _worker.Execute(input);
            //Extract output
            Tensor result = _worker.PeekOutput();
            result.Flatten();
            var output = Tensor2Graph(result);
            _worker.Dispose();
            return output;
        }

        //Latent generation
        public GAGenData Rand2Model()
        {
            GAGenData output = ScriptableObject.CreateInstance<GAGenData>();
            return output;
        }

        //Latent interpolation
        public GAGenData Interp()
        {
            GAGenData output = ScriptableObject.CreateInstance<GAGenData>();
            return output;
        }

        Texture2D LoadImage(string path)
        {
            Texture2D texture = null;
            byte[] data;

            if (File.Exists(path))
            {
                data = File.ReadAllBytes(path);
                texture = new Texture2D(2, 2, TextureFormat.BGRA32, false);
                texture.LoadImage(data);
                texture.Resize(256, 256);
            }

            return texture;
        }

        GAGenData Tensor2Graph(Tensor result)
        {
            List<GAGenNodeData> newNodes = new List<GAGenNodeData>();
            for (int i = 0; i < 12; i++)
            {
                GAGenNodeData newNodeData = new GAGenNodeData();
                newNodeData.ID = Guid.NewGuid().ToString();
                newNodeData.NodeType = Graph.NodeType.Segment;
                NodeSetting newSettings = new NodeSetting();
                newSettings.parameters.size = new Vector3(result[i * 12], result[i * 12 + 1], 1f);
                newSettings.parameters.curveParams.controlPoint = new Vector2(result[i * 12 + 2], 0f);
                newSettings.parameters.curveParams.tipOffset = new Vector2(result[i * 12 + 3], 0f);
                newSettings.parameters.nLoops = 5;
                newSettings.parameters.relativeBackwardTaper = result[i * 12 + 4];
                newSettings.parameters.relativeForwardTaper = result[i * 12 + 5];
                newNodeData.Settings = new NodeSetting();
                newNodes.Add(newNodeData);
            }
            GAGenData output = ScriptableObject.CreateInstance<GAGenData>();
            output.Nodes = newNodes;
            return output;
        }
    }
}
