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

        public NodesAndEdges GetNodesAndEdges(int subdiv)
        {
            NodesAndEdges nodesAndEdges = new NodesAndEdges();
            List<SElement> newNodes = new List<SElement>();
            List<SwordCreator.NestedList> newEdges = new List<SwordCreator.NestedList>();
            Dictionary<string, int> indexLookup = new Dictionary<string, int>();

            //Nodes
            for (int i = 0; i < Nodes.Count; i++)
            {
                SequentialNodeParams newParams = Nodes[i].Settings.parameters;
                var newNode = new STransit();
                newNode.Build(subdiv, newParams);
                newNodes.Add(newNode);
                indexLookup.Add(Nodes[i].ID, i);
            }

            //Edges
            for (int i = 0; i < Nodes.Count; i++)
            {
                SwordCreator.NestedList newEdgeSet = new SwordCreator.NestedList();
                newEdgeSet.val = new List<int>();
                for (int j = 0; j < Nodes[i].Connections.Count; j++)
                {
                    newEdgeSet.val.Add(indexLookup[Nodes[i].Connections[j].iD]);
                }
                newEdges.Add(newEdgeSet);
            }
            return nodesAndEdges;
        }

        public struct NodesAndEdges
        {
            public SElement[] nodes;
            public List<SwordCreator.NestedList> edges;
        }
    }
}