using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class TestingBase : MonoBehaviour
{
    [System.NonSerialized] public NNModel activeModel;
    [System.NonSerialized] public int selectedModelIndex = 0;
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
        if (activeModel == null)
        {
            if (models.Length == 0 || models[selectedModelIndex] == null) { return; }
            activeModel = models[selectedModelIndex];
        }

        Init();

        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _runtimeModel);

        //Tensor input = Preprocess();
        ////Debug.Log(input);
        //_worker.Execute(input);
        //Tensor output = _worker.PeekOutput("output1");
        //outputImage = Postprocess(output);
        //imageTarget.sprite = Sprite.Create(outputImage, new Rect(Vector3.zero, new Vector3(outputImage.width, outputImage.width)), Vector2.zero);
        ////Material mat = new Material(mRenderer.material);
        ////mat.mainTexture = outputImage;
        ////mRenderer.material = mat;


        //input.Dispose();
        //_worker?.Dispose();
    }
}
