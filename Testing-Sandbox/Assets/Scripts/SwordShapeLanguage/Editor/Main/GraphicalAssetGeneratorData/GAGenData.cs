using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SSL.Graph;
using SSL.Data.Utils;
using System.Linq;
using UnityEditor;

namespace SSL.Data
{
    [CreateAssetMenu(menuName = "Graphical Asset Generator")]
    public class GAGenData : ScriptableObject
    {
        [field: SerializeField] public List<GAGenNodeData> Nodes { get; set; } = new List<GAGenNodeData>();

        public void Save(GraphicalAssetGraphView graphView)
        {
            List<GraphViewNode> nodes = graphView.Nodes;
            Nodes = new List<GAGenNodeData>();
            foreach (GraphViewNode node in nodes)
            {
                Nodes.Add(GAGenDataUtils.GraphNodeToNodeData(node));
            }
            EditorUtility.SetDirty(this);
        }
    }
}