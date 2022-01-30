using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;
using System.IO;

public class TestingResNet : MonoBehaviour
{
    [System.NonSerialized] public NNModel activeModel;
    [System.NonSerialized] public int selectedModelIndex = 0;
    public Texture2D inputImage;
    public Text textTarget1, textTarget2, textTarget3, textTarget4, textTarget5;
    public Image imageTarget;
    public NNModel[] models;
    public TextAsset wordsText;
    Dictionary<int, string> wordsLookup;
    Model _runtimeModel;
    IWorker _worker;

    private void Init()
    {
        _runtimeModel = ModelLoader.Load(activeModel);
        LoadLookup();
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
        Tensor output = _worker.PeekOutput(_runtimeModel.outputs[_runtimeModel.outputs.Count - 1]);         //"resnetv22_dense0_fwd"
        Postprocess(output);
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
        //Test
        Texture2D tex = new Texture2D(inputImage.height, inputImage.width);

        float[] mean = new float[] { 0.485f, 0.456f, 0.406f };
        float[] stdd = new float[] { 0.229f, 0.224f, 0.225f };

        for (int i = 0; i < newTensor.height; i++)
        {
            for (int j = 0; j < newTensor.width; j++)
            {
                newTensor[0, i, j, 0] = (inputImage.GetPixel(i,j).r - mean[0]) / stdd[0];
                newTensor[0, i, j, 1] = (inputImage.GetPixel(i,j).g - mean[1]) / stdd[1];
                newTensor[0, i, j, 2] = (inputImage.GetPixel(i,j).b - mean[2]) / stdd[2];

                //Test
                tex.SetPixel(
                    i,
                    j,
                    new Color(
                        newTensor[0, i, j, 0],
                        newTensor[0, i, j, 1],
                        newTensor[0, i, j, 2]
                    ));
            }
        }
        tex.Apply();
        imageTarget.sprite = Sprite.Create(tex, new Rect(Vector3.zero, new Vector3(tex.width, tex.height)), Vector2.zero);
        return newTensor;
    }

    struct IndexValuePair
    {
        public int index;
        public float value;

        public IndexValuePair(int id, float val)
        {
            index = id;
            value = val;
        }
    }

    int IndexValuePairComparer(IndexValuePair a, IndexValuePair b)
    {
        if(a.value < b.value)
        {
            return -1;
        }
        else if(a.value > b.value)
        {
            return 1;
        }
        return 0;
    }

    void Postprocess(Tensor tensor)
    {
        //Debug.Log(tensor);
        List<IndexValuePair> indexList = new List<IndexValuePair>();
        for (int i = 0; i < tensor.length; i++)
        {
            indexList.Add(new IndexValuePair(i, tensor[i]));
        }
        indexList.Sort(IndexValuePairComparer);
        textTarget1.text = $"1: {wordsLookup[indexList[indexList.Count-1].index]} ({indexList[indexList.Count-1].value})";
        textTarget2.text = $"2: {wordsLookup[indexList[indexList.Count-2].index]} ({indexList[indexList.Count-2].value})";
        textTarget3.text = $"3: {wordsLookup[indexList[indexList.Count-3].index]} ({indexList[indexList.Count-3].value})";
        textTarget4.text = $"4: {wordsLookup[indexList[indexList.Count-4].index]} ({indexList[indexList.Count-4].value})";
        textTarget5.text = $"5: {wordsLookup[indexList[indexList.Count-5].index]} ({indexList[indexList.Count-5].value})";
    }

    void LoadLookup()
    {
        wordsLookup = new Dictionary<int, string>();
        string text = wordsText.text;
        string[] splitText = text.Split(new string[] { "\n" }, System.StringSplitOptions.None);
        for (int i = 0; i < splitText.Length; i++)
        {
            wordsLookup.Add(i, splitText[i]);
        }
    }
}

