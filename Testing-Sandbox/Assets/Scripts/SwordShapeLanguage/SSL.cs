using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SSL
{
    /// <summary>
    /// The sword graph is a directed graph representing the 
    /// elements of a sword as they connect to one another.
    /// 
    /// Liminal elements  (in the middle / transitional e.g. blade, grip)
	///     Params: subdiv(int), size(vect3), nLoops(int), deforms(vect3[nLoops]), splineParams(spParams), nextElement(element)
    ///     
    /// Union elements(branching e.g.crossguard)
    ///     Params: subdiv(int), size(vect3), nLoops(int), nextElements(element[6])
    ///     
    /// Terminal elements(at ends e.g.pommel and tip)
    ///     Params: subdiv(int), size(vect3), nLoops(int), nextElement(only relevant for first)
    ///     
    ///Note:
    ///     nLoops + 2 (where end loops are non deformable)
    /// 
    /// </summary>
    public class SwordGraph
    {
        SElement[] _nodes;
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
            Dictionary<SElement, bool> completionLookup = new Dictionary<SElement, bool>();
            Stack<SElement> stack = new Stack<SElement>();
            int currentNodeIndex = _firstNode;
            stack.Push(_nodes[currentNodeIndex]);

            //Populate completion lookup
            for (int i = 0; i < _nodes.Length; i++)
            {
                completionLookup.Add(_nodes[i], false);
            }

            //Iterate over all edges and merge all meshes
            while (stack.Count > 0) 
            {
                int[] currentNodeConnections = _edges[currentNodeIndex];
                SElement currentNode = _nodes[currentNodeIndex];
                if (completionLookup[currentNode] == false)
                {
                    //Get mesh
                    Mesh subMesh = currentNode.GetMesh();

                    //Merge into one mesh and reposition m2
                    CombineInstance[] combine = new CombineInstance[2];
                    combine[0].mesh = newMesh;
                    combine[1].mesh = subMesh;
                    //combine[1].transform = some math magic

                    newMesh.CombineMeshes(combine);
                    completionLookup[currentNode] = true;
                }

                bool nextFound = false;
                for (int i = 0; i < currentNodeConnections.Length; i++)
                {
                    if (!completionLookup[_nodes[currentNodeConnections[i]]])
                    {
                        currentNodeIndex = currentNodeConnections[i];
                        stack.Push(_nodes[currentNodeIndex]);
                        nextFound = true;
                        break;
                    }
                }

                if (!nextFound)
                    stack.Pop();
            }
            return newMesh;
        }
    }

    public struct SplineParams
    {

    }

    public abstract class SElement
    {
        protected Mesh mesh;
        public Mesh GetMesh() => mesh;
    }

    public class SLiminal : SElement
    {

        public void Build(int subdiv, Vector3 size, int nLoops, Vector3[] deforms, SplineParams sParams)
        {
            throw new System.NotImplementedException();
        }
    }

    public class SUnion : SElement
    {

        public void Build(int subdiv, Vector3 size, int nLoops)
        {
            throw new System.NotImplementedException();
        }
    }

    public class STerminal : SElement
    {

        public void Build(int subdiv, Vector3 size, int nLoops)
        {
            throw new System.NotImplementedException();
        }
    }
}
