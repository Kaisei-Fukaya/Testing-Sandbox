using System;
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
        Dictionary<int, int[]> _edges;

        //Parser?
        public void Build()
        {

        }

        public void Init()
        {
            SLiminal tester = new SLiminal();
            _nodes = new SElement[] { tester };
            _edges = new Dictionary<int, int[]>();
            _edges.Add(0, new int[0]);
            var offsets = new Vector3[] { 
                new Vector3(1.5f, 0f, .5f), 
                Vector3.zero, 
                new Vector3(-1.5f, 0f, .5f),
                Vector3.zero,
                new Vector3(-1.5f, 0f, -.5f),
                Vector3.zero,
                new Vector3(1.5f, 0f, -.5f),
                Vector3.zero
            };
            tester.Build(1, new Vector3(10f, 30f, 2f), 10, offsets, new SplineParams());
        }

        public Mesh Generate()
        {
            if (!(_nodes.Length > 0))
                return new Mesh();

            Mesh newMesh = new Mesh();
            Dictionary<SElement, bool> completionLookup = new Dictionary<SElement, bool>();
            Stack<SElement> stack = new Stack<SElement>();
            int currentNodeIndex = 0;
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
                    return subMesh;
                    //Merge into one mesh and reposition m2
                    CombineInstance[] combine = new CombineInstance[1];
                    combine[0].mesh = subMesh;
                    //combine[1].transform = some math magic
                    newMesh.CombineMeshes(combine);
                    Debug.Log(newMesh.vertexCount);
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
            mesh = new Mesh();
            List<Vector3> allVerts = new List<Vector3>();
            int loopLen = 4 * (int)Math.Pow(2, subdiv);

            //Ensure deforms matches looplen
            if(deforms.Length != loopLen)
            {
                List<Vector3> d = new List<Vector3>(deforms);
                if (d.Count > loopLen)
                    d.RemoveRange(loopLen, d.Count);
                else if (d.Count < loopLen)
                {
                    int diff = loopLen - d.Count;
                    for (int i = 0; i < diff; i++)
                    {
                        d.Add(Vector3.zero);
                    }
                }
                deforms = d.ToArray();
            }
            Debug.Log(deforms[0]);
            //Create Verts
            for (int i = 0; i < nLoops; i++)
            {
                Vector3[] verts = new Vector3[loopLen];

                float yOffset = (size.y / nLoops) * (i + 1);
                int quartOfLength = verts.Length / 4;
                verts[0]                 = new Vector3(-size.x/2, yOffset, -size.z/2) + deforms[0];
                verts[quartOfLength]     = new Vector3( size.x/2, yOffset, -size.z/2) + deforms[quartOfLength];
                verts[quartOfLength * 2] = new Vector3( size.x/2, yOffset,  size.z/2) + deforms[quartOfLength * 2];
                verts[quartOfLength * 3] = new Vector3(-size.x/2, yOffset,  size.z/2) + deforms[quartOfLength * 3];
                //Debug.Log($"vertsL= {verts.Length}");
                //Between 0-1
                for (int j = 1; j < quartOfLength; j++)
                {
                    Vector3 baseOffset = new Vector3(
                        (-size.x / 2) + ((size.x / quartOfLength) * j),
                        yOffset,
                        -size.z / 2
                        );
                    verts[j] = baseOffset + deforms[j];
                    //Debug.Log($"j1= {j}");
                }
                //Between 1-2
                for (int j = quartOfLength + 1; j < quartOfLength * 2; j++)
                {
                     Vector3 baseOffset = new Vector3(
                        size.x / 2,
                        yOffset,
                        (-size.z / 2) + ((size.z / quartOfLength) * (j - quartOfLength))
                        );
                    verts[j] = baseOffset + deforms[j];
                    //Debug.Log($"j2= {j}");
                }
                //Between 2-3
                for (int j = (quartOfLength * 2) + 1; j < quartOfLength * 3; j++)
                {
                    Vector3 baseOffset = new Vector3(
                        (size.x / 2) - ((size.x / quartOfLength) * (j - (quartOfLength * 2))),
                        yOffset,
                        size.z / 2
                        );
                    verts[j] = baseOffset + deforms[j];
                    //Debug.Log($"j3= {j}");
                }
                //Between 3-0
                for (int j = (quartOfLength * 3) + 1; j < verts.Length; j++)
                {
                    verts[j] = new Vector3(
                        -size.x / 2,
                        yOffset,
                        (size.z / 2) - ((size.z / quartOfLength) * (j - (quartOfLength * 3)))
                        );
                    //Debug.Log($"j4= {j}");
                }
                allVerts.AddRange(verts);
            }

            //Create Tris
            List<int> allTris = new List<int>();
            for (int i = 1; i < nLoops; i++)
            {
                for (int j = 0; j < loopLen; j++)
                {
                    int nxt1 = (loopLen * i) + (j + 1);
                    if (nxt1 >= loopLen * (i + 1))
                        nxt1 = loopLen * i;

                    int nxt2 = (loopLen * (i - 1)) + (j + 1);
                    if (nxt2 >= loopLen * i)
                        nxt2 = loopLen * (i - 1);

                    //Clockwise!!!
                    int[] tris = new int[] { (loopLen * i) + j, nxt1, nxt2,
                                             (loopLen * i) + j, nxt2, (loopLen * (i - 1)) + j };

                    /*  //Counter-clockwise
                    int prev = (loopLen * i) + (j - 1);

                    if (prev == -1)
                        prev = loopLen - 1;

                    int[] tris = new int[] { (loopLen * i) + j, prev                   , (loopLen * (i - 1)) + j,
                                             (loopLen * i) + j, (loopLen * (i - 1)) + j, (loopLen * (i - 1)) + (j + 1) };
                    */

                    allTris.AddRange(tris);
                }
            }


            mesh.vertices  = allVerts.ToArray();
            mesh.triangles = allTris.ToArray();
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
