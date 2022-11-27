using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SSL;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class SwordCreator : MonoBehaviour
{
    void Start()
    {
        SwordGraph sg = new SwordGraph();
        sg.Init();
        Mesh m = sg.Generate();
        GetComponent<MeshFilter>().mesh = m;

    }

}
