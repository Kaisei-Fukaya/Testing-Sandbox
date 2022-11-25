using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunOptimisation : MonoBehaviour
{
    [SerializeField] PSO.Parameters _optimisationParameters;

    [SerializeField] Color _targetColour;

    MeshRenderer _meshRenderer;

    Material _material;

    public void Run()
    {
        if (_meshRenderer == null)
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _material = _meshRenderer.sharedMaterial;
        PSO.Optimise(
            this,  
            ColourFitness,
            ColourRenderFunction,
            _optimisationParameters
        );
    }

    public void ColourRenderFunction(float[] val, int iteration)
    {
        Vector3 valVector = new Vector3();
        for (int i = 0; i < val.Length; i++)
        {
            valVector[i] = val[i];
        }
        valVector.Normalize();
        _material.color = new Color(valVector.x, valVector.y, valVector.z);
        Debug.Log($"iteration: {iteration}");
    }

    public float ColourFitness(float[] value)
    {
        float fitness = 0f;
        float r = _targetColour.r;
        float g = _targetColour.g;
        float b = _targetColour.b;
        //Calc diff
        float rDiff = r - value[0];
        float gDiff = g - value[1];
        float bDiff = b - value[2];
        //Ensure diff values are always positive
        if (rDiff < 0f)
            rDiff = rDiff * -1;
        if (gDiff < 0f)
            gDiff = gDiff * -1;
        if (bDiff < 0f)
            bDiff = bDiff * -1;
        //Sum diffs
        float totalDiff = rDiff + gDiff + bDiff;
        fitness = 765 - totalDiff;
        return fitness;
    }
}
