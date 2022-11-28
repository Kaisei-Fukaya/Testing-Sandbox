using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SSL;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class SwordCreator : MonoBehaviour
{
    [Range(0, 5)]
    [SerializeField] int _subdiv;
    [Range(2, 20)]
    [SerializeField] int _loops;
    [SerializeField] Vector3 _size = Vector3.one;
    [SerializeField] List<Vector3> _deforms;

    SwordGraph sg;
    MeshFilter mf;

    void Start()
    {
        Gen();
    }

    void Gen()
    {
        if (sg == null)
            sg = new SwordGraph();
        if (mf == null)
            mf = GetComponent<MeshFilter>();
        sg.Build(_subdiv, _size, _loops, _deforms.ToArray(), new SplineParams());
        Mesh m = sg.Generate();
        mf.mesh = m;
    }

    void OnValidate()
    {
        Gen();
    }
}