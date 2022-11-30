using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SSL;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class SwordCreator : MonoBehaviour
{
    [Range(0, 5)]
    [SerializeField] int _subdiv;
    [SerializeField] List<Vector3> _deforms;
    [SerializeField] float _spacing;
    [SerializeField] STransit[] _nodes;
    [SerializeField] List<NestedList> _edges;

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

        BuildNodes();

        Dictionary<int, int[]> edgesConverted = new Dictionary<int, int[]>();
        for (int i = 0; i < _edges.Count; i++)
        {
            edgesConverted.Add(i, _edges[i].val.ToArray());
        }

        sg.Load(_subdiv, _spacing, _nodes, edgesConverted);
        Mesh m = sg.Generate();
        mf.mesh = m;
    }

    void BuildNodes()
    {
        for (int i = 0; i < _nodes.Length; i++)
        {
            if (_nodes[i] is STransit)
            {
                var n = (STransit)_nodes[i];
                n.Build(_subdiv);
            }
        }
    }

    void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall -= Gen;
        UnityEditor.EditorApplication.delayCall += Gen;
    }

    [System.Serializable]
    public class NestedList
    {
        public List<int> val;
    }
}