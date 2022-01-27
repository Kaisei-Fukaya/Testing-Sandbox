using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;

public class TestingStyleTransfer : MonoBehaviour
{
    [System.NonSerialized] public NNModel activeModel;
    [System.NonSerialized] public int selectedModelIndex = 0;
    public Texture2D inputImage;
    public Image imageTarget;
    public Texture2D outputImage;
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
        Tensor output = _worker.PeekOutput("output1");
        outputImage = Postprocess(output);
        imageTarget.sprite = Sprite.Create(outputImage, new Rect(Vector3.zero, new Vector3(outputImage.width, outputImage.width)), Vector2.zero);
        //Material mat = new Material(mRenderer.material);
        //mat.mainTexture = outputImage;
        //mRenderer.material = mat;


        input.Dispose();
        _worker?.Dispose();
    }

    Tensor Preprocess()
    {
        Tensor newTensor = new Tensor(1, inputImage.height, inputImage.width, 3);
        for (int i = 0; i < newTensor.height; i++)
        {
            for (int j = 0; j < newTensor.width; j++)
            {
                newTensor[0, i, j, 0] = PixFloatTo255(inputImage.GetPixel(i,j).r);
                newTensor[0, i, j, 1] = PixFloatTo255(inputImage.GetPixel(i,j).g);
                newTensor[0, i, j, 2] = PixFloatTo255(inputImage.GetPixel(i,j).b);
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

    Texture2D Postprocess(Tensor tensor)
    {
        //Debug.Log(tensor);
        Texture2D tex = new Texture2D(tensor.width, tensor.height);
        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                tex.SetPixel(
                    i, 
                    j, 
                    new Color(
                        ToPixFloat((int)tensor[0, i, j, 0]), 
                        ToPixFloat((int)tensor[0, i, j, 1]), 
                        ToPixFloat((int)tensor[0, i, j, 2])
                        )
                    );
                //print(ToPixFloat((int)tensor[0, i, j, 0]));
            }
        }
        tex.Apply();
        return tex;
    }

    
}
