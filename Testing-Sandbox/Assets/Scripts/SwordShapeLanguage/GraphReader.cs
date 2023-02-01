using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SSL;
using SSL.Data;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class GraphReader : MonoBehaviour
{
    [Range(0, 5)]
    [SerializeField] int _subdiv;
    [SerializeField] bool _useFlatshading = true;

    [SerializeField] public GAGenData data;

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
        if (data == null)
            return;
        GAGenData.NodesAndEdges nodesAndEdges = data.GetNodesAndEdges(_subdiv);

        //BuildNodes();

        sg.Load(_subdiv, 1f, nodesAndEdges.nodes, nodesAndEdges.edges, _useFlatshading);
        Mesh m = sg.Generate();
        mf.mesh = m;
    }

    //void BuildNodes(SElement[] nodes)
    //{
    //    for (int i = 0; i < nodes.Length; i++)
    //    {
    //        nodes[i].Build(_subdiv);
    //    }
    //    _testTerminalNode.Build(_subdiv);
    //}


    void OnValidate()
    {
        UpdateMesh();
    }

    public void UpdateMesh()
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