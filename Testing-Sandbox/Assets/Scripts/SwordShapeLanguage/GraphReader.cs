using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SSL;
using SSL.Data;
using UnityEditor;

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

        sg.Load(_subdiv, 1f, nodesAndEdges.nodes, nodesAndEdges.edges, _useFlatshading);
        Mesh m = sg.Generate();
        mf.mesh = m;
    }

    void OnValidate()
    {
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        UnityEditor.EditorApplication.delayCall -= Gen;
        UnityEditor.EditorApplication.delayCall += Gen;
    }

    public void SaveMesh()
    {
        string path = EditorUtility.SaveFilePanel("Save Mesh", "Assets/", name, "asset");
        if (string.IsNullOrEmpty(path)) { return; }

        path = FileUtil.GetProjectRelativePath(path);

        AssetDatabase.CreateAsset(mf.sharedMesh, path);
        AssetDatabase.SaveAssets();
    }

    public List<SElement.BezierPoint> GetBezierPoints()
    {
        Debug.Log("aaaa");
        return sg.GetBezierPoints();
    }

    [System.Serializable]
    public class NestedList
    {
        public List<int> val;
    }
}