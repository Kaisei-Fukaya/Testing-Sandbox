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
        int _subdiv;
        float _elementSpacing;

        //Parser?
        public void Load(int subdivisions, float elementSpacing, SElement[] nodes, Dictionary<int, int[]> edges)
        {
            _nodes = nodes;
            _edges = edges;
            _subdiv = subdivisions;
            _elementSpacing = elementSpacing;
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
                    newMesh = JoinMeshes(newMesh, subMesh, _elementSpacing, _subdiv);
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
            newMesh.RecalculateNormals();
            return newMesh;
        }

        Mesh JoinMeshes(Mesh meshA, Mesh meshB, float elementSpacing, int subdiv)
        {
            int vertCount = meshA.vertexCount;

            if(vertCount == 0)
            {
                return meshB;
            }

            //Merge into one mesh and reposition mb

            Mesh outMesh = new Mesh();
            List<Vector3> newVerts = new List<Vector3>(meshA.vertices);
            Vector3[] bVerts = meshB.vertices;
            for (int i = 0; i < bVerts.Length; i++)
            {
                bVerts[i].y += (newVerts[newVerts.Count - 1].y + elementSpacing);
            }
            int[] bTriangs = meshB.triangles;
            for (int i = 0; i < bTriangs.Length; i++)
            {
                bTriangs[i] += vertCount;
            }
            newVerts.AddRange(bVerts);
            outMesh.vertices = newVerts.ToArray();

            int loopLen = 4 * (int)Math.Pow(2, subdiv);
            List<int> newTriangs = new List<int>(meshA.triangles);
            newTriangs.AddRange(bTriangs);
            for (int i = 0; i < loopLen; i++)
            {
                int nxt1 = (vertCount) + (i + 1);
                if (nxt1 >= vertCount + loopLen)
                    nxt1 = vertCount;

                int nxt2 = (vertCount - loopLen) + (i + 1);
                if (nxt2 >= vertCount)
                    nxt2 = vertCount - loopLen;

                newTriangs.AddRange(new int[6] { (vertCount) + i, nxt1, nxt2,
                                                 (vertCount) + i, nxt2, (vertCount - loopLen) + i });
            }
            outMesh.triangles = newTriangs.ToArray();
            //Debug.Log(newMesh.vertexCount);
            return outMesh;
        }
    }

    [Serializable]
    public struct TransitNodeParams
    {
        public int nLoops;
        public Vector3 size;
        public Vector3[] deforms;
        public SplineParams sParams;
        public TransitNodeParams(int loops, Vector3 size, Vector3[] deformations, SplineParams splineParams)
        {
            this.nLoops = loops;
            this.size = size;
            this.deforms = deformations;
            this.sParams = splineParams;
        }
    }

    public struct SplineParams
    {

    }

    public abstract class SElement
    {
        protected Mesh mesh;
        public Mesh GetMesh() => mesh;

        protected Vector3[] Redeform(Vector3[] deforms, int targetLength)
        {
            Vector3[] newD = new Vector3[targetLength];
            int factorDiff = 0;
            int dLen = deforms.Length;
            int counter = 0;
            while (dLen != targetLength)
            {
                if (counter > 10)
                {
                    dLen = targetLength;
                    throw new Exception("deforms is not an acceptable length");
                }
                if (dLen > targetLength)
                {
                    dLen /= 2;
                    factorDiff--;
                }
                else if (dLen < targetLength)
                {
                    dLen *= 2;
                    factorDiff++;
                }
                counter++;
            }

            //Reduce
            if (factorDiff < 0)
            {
                //Debug.Log(factorDiff);
                int factorDiffPositive = factorDiff * -1;
                int interval = (int)Math.Pow(2, factorDiffPositive);
                int index = 0;
                for (int i = 0; i < deforms.Length; i += interval)
                {
                    newD[index] = deforms[i];
                    index++;
                }
            }
            //Interpolate
            else if (factorDiff > 0)
            {
                int interval = (int)Math.Pow(2, factorDiff);
                int index = 0;
                int[] keyVerts = new int[deforms.Length];
                for (int i = 0; i < newD.Length; i++)
                {
                    if (i % interval == 0)
                    {
                        keyVerts[index] = i;
                        newD[i] = deforms[index];
                        index++;
                    }
                }

                for (int i = 0; i < keyVerts.Length; i++)
                {
                    int nextKV = i + 1;
                    if (nextKV == keyVerts.Length)
                    {
                        nextKV = 0;
                        for (int j = 1; j < keyVerts[1]; j++)
                        {
                            newD[keyVerts[i] + j] = new Vector3(
                                Mathf.Lerp(newD[keyVerts[i]].x, newD[keyVerts[nextKV]].x, (1f / interval) * (j)),
                                Mathf.Lerp(newD[keyVerts[i]].y, newD[keyVerts[nextKV]].y, (1f / interval) * (j)),
                                Mathf.Lerp(newD[keyVerts[i]].z, newD[keyVerts[nextKV]].z, (1f / interval) * (j)));
                        }
                    }
                    else
                    {
                        for (int j = keyVerts[i] + 1; j < keyVerts[nextKV]; j++)
                        {
                            newD[j] = new Vector3(
                                Mathf.Lerp(newD[keyVerts[i]].x, newD[keyVerts[nextKV]].x, (1f / interval) * (j - keyVerts[i] + 1)),
                                Mathf.Lerp(newD[keyVerts[i]].y, newD[keyVerts[nextKV]].y, (1f / interval) * (j - keyVerts[i] + 1)),
                                Mathf.Lerp(newD[keyVerts[i]].z, newD[keyVerts[nextKV]].z, (1f / interval) * (j - keyVerts[i] + 1)));
                        }
                    }
                }
            }
            return newD;
        }
    }

    [Serializable]
    public class STransit : SElement
    {
        [SerializeField] TransitNodeParams storedParameters;

        public void Build(int subdivs)
        {
            Build(subdivs, storedParameters);
        }
        public void Build(int subdiv, TransitNodeParams parameters)
        {
            Build(subdiv, parameters.size, parameters.nLoops, parameters.deforms, parameters.sParams);
        }

        public void Build(int subdiv, Vector3 size, int nLoops, Vector3[] deforms, SplineParams sParams)
        {
            mesh = new Mesh();
            List<Vector3> allVerts = new List<Vector3>();
            int loopLen = 4 * (int)Math.Pow(2, subdiv);
            nLoops += 2;

            //Ensure deforms matches looplen
            if(deforms.Length != loopLen)
            {
                deforms = Redeform(deforms, loopLen);
            }
            //Debug.Log(deforms[0]);
            //Create Verts
            for (int i = 0; i < nLoops; i++)
            {
                Vector3[] verts = new Vector3[loopLen];

                float yOffset = (size.y / (nLoops-1)) * i;
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
                    Vector3 baseOffset = new Vector3(
                        -size.x / 2,
                        yOffset,
                        (size.z / 2) - ((size.z / quartOfLength) * (j - (quartOfLength * 3)))
                        );
                    verts[j] = baseOffset + deforms[j];
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
