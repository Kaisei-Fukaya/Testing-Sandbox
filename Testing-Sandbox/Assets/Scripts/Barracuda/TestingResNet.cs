using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;

public class TestingResNet : MonoBehaviour
{
    [System.NonSerialized] public NNModel activeModel;
    [System.NonSerialized] public int selectedModelIndex = 0;
    public Texture2D inputImage;
    public Text textTarget;
    public NNModel[] models;
    Model _runtimeModel;
    IWorker _worker;

    private void Init()
    {
        _runtimeModel = ModelLoader.Load(activeModel);
    }

    public void Run()
    {
        //if (_runtimeModel == null)
        //{
        //    if (model == null)
        //    {
        //        return;
        //    }
        //    Init();
        //}
        if(activeModel == null)
        {
            if(models.Length == 0 || models[selectedModelIndex] == null) { return; }
            activeModel = models[selectedModelIndex];
        }

        Init();

        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _runtimeModel);

        Tensor input = Preprocess();
        //Debug.Log(input);
        _worker.Execute(input);
        Tensor output = _worker.PeekOutput("resnetv22_dense0_fwd");
        textTarget.text = Postprocess(output);
        //outputImage = Postprocess(output);
        //Material mat = new Material(mRenderer.material);
        //mat.mainTexture = outputImage;
        //mRenderer.material = mat;


        input.Dispose();
        _worker?.Dispose();
    }

    Tensor Preprocess()
    {
        Tensor newTensor = new Tensor(1, inputImage.height, inputImage.width, 3);

        float[] mean = new float[] { 0.485f, 0.456f, 0.406f };
        float[] stdd = new float[] { 0.229f, 0.224f, 0.225f };

        for (int i = 0; i < newTensor.height; i++)
        {
            for (int j = 0; j < newTensor.width; j++)
            {
                newTensor[0, i, j, 0] = (inputImage.GetPixel(i,j).r - mean[0]) / stdd[0];
                newTensor[0, i, j, 1] = (inputImage.GetPixel(i,j).g - mean[1]) / stdd[1];
                newTensor[0, i, j, 2] = (inputImage.GetPixel(i,j).b - mean[2]) / stdd[2];
            }
        }
        return newTensor;
    }

    int PixFloatTo255(float val)
    {
        return (int)(val * 255);
    }

    float ToPixFloat(int val)
    {
        return val / 255f;
    }

    string Postprocess(Tensor tensor)
    {
        //Debug.Log(tensor);
        string text = "";
        int currentHighestIndex = 0;
        float currentHighest = 0f;
        for (int i = 0; i < tensor.length; i++)
        {
            if(tensor[i] > currentHighest)
            {
                currentHighest = tensor[i];
                currentHighestIndex = i;
            }
        }
        text = currentHighestIndex.ToString();
        return text;
    }

    Dictionary<int, string> resNetLookup = new Dictionary<int, string>()
    {

    };

    
}

