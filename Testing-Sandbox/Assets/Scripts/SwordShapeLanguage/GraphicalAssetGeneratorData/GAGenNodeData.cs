using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SSL.Graph;

namespace SSL.Data
{
    [Serializable]
    public class GAGenNodeData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public NodeType NodeType { get; set; }
        [field: SerializeField] public List<ConnectionData> OutGoingConnections { get; set; }
        [field: SerializeField] public List<ConnectionData> InGoingConnections { get; set; }
        [field: SerializeField] public NodeSetting Settings { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }

    [Serializable]
    public struct ConnectionData
    {
        public string iD;
        public int indexInOwner, indexInOther;
        public string owner;
        public GAPortType type;
        public string ownerDisplayName;

        public ConnectionData(string iD, int indexInOther, int indexInOwner, string owner, GAPortType type, string displayName = "")
        {
            this.iD = iD;
            this.indexInOwner = indexInOwner;
            this.indexInOther = indexInOther;
            this.owner = owner;
            this.type = type;
            ownerDisplayName = displayName;
        }
    }

    [Serializable]
    public class NodeSetting
    {
        //Number of inputs
        public List<GAPortType> i_portTypes = new List<GAPortType>();
        //Output settings
        public List<GAPortType> o_portTypes = new List<GAPortType>();

        public SequentialNodeParams parameters = SequentialNodeParams.defaultParams;
    }
}

namespace SSL.Graph
{
    public enum NodeType
    {
        Sequential,
        Branch
    }

    public enum GAPortType
    {
        Mesh
    }
}