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
        NNModel _modelAssetEncoder, _modelAssetBottleneck, _modelAssetDecoder;
        Model _modelEncoder, _modelBottleneck, _modelDecoder;
        IWorker _workerEncoder, _workerBottleneck, _workerDecoder;
        System.Random _random;

        public Inference()
        {
            _modelAssetEncoder = (NNModel)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/ONNX/model_encoder.onnx", typeof(NNModel));
            _modelEncoder = ModelLoader.Load(_modelAssetEncoder);
            _workerEncoder = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _modelEncoder);

            _modelAssetBottleneck = (NNModel)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/ONNX/model_bottleneck.onnx", typeof(NNModel));
            _modelBottleneck = ModelLoader.Load(_modelAssetBottleneck);
            _workerBottleneck = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _modelBottleneck);

            _modelAssetDecoder = (NNModel)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/ONNX/model_decoder.onnx", typeof(NNModel));
            _modelDecoder = ModelLoader.Load(_modelAssetDecoder);
            _workerDecoder = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _modelDecoder);
            _random = new System.Random();
        }

        //Image to Model
        public GAGenData Img2Model(string path)
        {
            Texture2D image = LoadImage(path);
            Tensor input = new Tensor(image);
            var output = ReconstructInference(input);
            return output;
        }

        //Latent generation
        public GAGenData Rand2Model()
        {
            Tensor input = new Tensor(new int[] { 1, 512 });
            for (int i = 0; i < input.length; i++)
            {
                input[i] = (float)_random.NextDouble();
            }
            var output = LatentInference(input);
            return output;
        }

        //Latent interpolation
        public GAGenData Interp(string pathA, string pathB, float t)
        {
            Texture2D imageA = LoadImage(pathA);
            Texture2D imageB = LoadImage(pathB);
            Tensor inputA = new Tensor(imageA);
            Tensor inputB = new Tensor(imageB);
            Tensor latentA = EncodeInference(inputA);
            Tensor latentB = EncodeInference(inputB);
            Tensor latentLerp = new Tensor(new int[] { 1, 512 });
            for (int i = 0; i < latentLerp.length; i++)
            {
                latentLerp[i] = Mathf.Lerp(latentA[i], latentB[i], t);
            }
            var output = LatentInference(latentLerp);
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
                texture.Resize(128, 128);
            }

            return texture;
        }

        GAGenData ReconstructInference(Tensor input)
        {
            _workerEncoder.Execute(input);
            Tensor result = _workerEncoder.PeekOutput();
            _workerBottleneck.Execute(result);
            result = _workerBottleneck.PeekOutput();
            _workerDecoder.Execute(result);
            result = _workerDecoder.PeekOutput();

            result.Flatten();
            var output = Tensor2Graph(result);
            _workerEncoder.Dispose();
            _workerBottleneck.Dispose();
            _workerDecoder.Dispose();
            return output;
        }

        GAGenData LatentInference(Tensor input)
        {
            _workerDecoder.Execute(input);
            Tensor result = _workerDecoder.PeekOutput();
            result.Flatten();
            var output = Tensor2Graph(result);
            _workerDecoder.Dispose();
            return output;
        }
        Tensor EncodeInference(Tensor input)
        {
            _workerEncoder.Execute(input);
            Tensor result = _workerEncoder.PeekOutput();
            var output = result.DeepCopy();
            _workerEncoder.Dispose();
            return output;
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

        Tensor Graph2Tensor(GAGenData data)
        {
            Tensor newTensor = new Tensor(new int[] { 1, 72 });
            var nodes = data.Nodes;
            for (int i = 0; i < 12; i++)
            {
                var currentParams = nodes[i].Settings.parameters;
                newTensor[i * 12] = currentParams.size.x;
                newTensor[i * 12 + 1] = currentParams.size.y;
                newTensor[i * 12 + 2] = currentParams.curveParams.controlPoint.x;
                newTensor[i * 12 + 3] = currentParams.curveParams.tipOffset.x;
                newTensor[i * 12 + 4] = currentParams.relativeBackwardTaper;
                newTensor[i * 12 + 5] = currentParams.relativeForwardTaper;
            }
            return newTensor;
        }
    }
}
