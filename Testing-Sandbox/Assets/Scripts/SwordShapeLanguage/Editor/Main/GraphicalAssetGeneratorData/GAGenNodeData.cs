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
        [field: SerializeField] public List<ConnectionData> GenConnections { get; set; }
        [field: SerializeField] public List<ConnectionData> TrainConnections { get; set; }
        [field: SerializeField] public NodeSetting AdditionalSettings { get; set; }
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
        public List<string> i_inputPaths = new List<string>();
        //Output settings
        public List<GAPortType> o_portTypes = new List<GAPortType>();
        public List<string> o_outputPaths = new List<string>();
        //Image2MeshNode
        public int i2m_dropdownOpt1;
        public int i2m_dropdownOpt2;
        //InputNode
        public string i_plainText;
        public int i_chosenInputMode;
        //Number Input
        public int n_min;
        public int n_max;
        public int n_val;
        //Interpolator
        public float slider1;

        public GAPortType l_selectedPortType;
    }

    [Serializable]
    public class OutputNodeSetting : NodeSetting
    {
        public OutputNodeSetting(List<GAPortType> portTypes, List<string> outputPaths)
        {
            o_portTypes = portTypes;
            o_outputPaths = outputPaths;
        }
    }
}