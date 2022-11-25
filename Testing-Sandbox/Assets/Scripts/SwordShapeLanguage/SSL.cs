using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSL
{
    /// <summary>
    /// The sword graph is a directed graph representing the 
    /// elements of a sword as they connect to one another.
    /// </summary>
    public class SwordGraph
    {
        SwordElement[] _nodes;
        int _firstNode;
        Dictionary<int, int[]> _edges;

        //Parser?
        public void Build()
        {

        }

        public Mesh Generate()
        {
            if (!(_nodes.Length > 0))
                return new Mesh();

            Mesh newMesh = new Mesh();
            Stack<SwordElement> stack;
            int currentNode = _firstNode;
            bool running = true;

            //Iterate over all edges and them. Breadth first search?
            while (running) 
            {
                int[] currentNodeConnections = _edges[currentNode];
                
                _nodes[currentNode].Build();

                for (int i = 0; i < currentNodeConnections.Length; i++)
                {

                }
            }
            return newMesh;
        }
    }

    public abstract class SwordElement
    {
        public abstract void Build();
        public abstract Mesh Generate();
    }
}