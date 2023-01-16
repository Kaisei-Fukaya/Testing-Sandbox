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
            if (!(_nodes.Length > 0) || _edges.Count < _nodes.Length)
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
    public struct NodeParams
    {
        [Min(0)]
        public int nLoops;
        [Min(0f)]
        public float rounding;
        public Vector3 size;
        public Vector3[] deforms;
        public SplineParams sParams;
        public NodeParams(int loops, float rounding, Vector3 size, Vector3[] deformations, SplineParams splineParams)
        {
            this.nLoops = loops;
            this.rounding = rounding;
            this.size = size;
            this.deforms = deformations;
            this.sParams = splineParams;
        }
    }

    public struct SplineParams
    {

    }

    [Serializable]
    public abstract class SElement
    {
        [SerializeField] NodeParams storedParameters;
        protected Mesh mesh;
        public Mesh GetMesh() => mesh;
        public virtual void Build(int subdivs)
        {
            Build(subdivs, storedParameters);
        }
        public void Build(int subdiv, NodeParams parameters)
        {
            Build(subdiv, parameters.rounding, parameters.size, parameters.nLoops, parameters.deforms, parameters.sParams);
        }
        public abstract void Build(int subdiv, float rounding, Vector3 size, int nLoops, Vector3[] deforms, SplineParams sParams);
        protected Vector3[] BuildLoop(int loopLen, int nLoops, int index, Vector3 size, Vector3[] deforms)
        {
            Vector3[] verts = new Vector3[loopLen];

            float yOffset = (size.y / (nLoops - 1)) * index;
            int quartOfLength = verts.Length / 4;
            verts[0] = new Vector3(-size.x / 2, yOffset, -size.z / 2) + deforms[0];
            verts[quartOfLength] = new Vector3(size.x / 2, yOffset, -size.z / 2) + deforms[quartOfLength];
            verts[quartOfLength * 2] = new Vector3(size.x / 2, yOffset, size.z / 2) + deforms[quartOfLength * 2];
            verts[quartOfLength * 3] = new Vector3(-size.x / 2, yOffset, size.z / 2) + deforms[quartOfLength * 3];
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
            return verts;
        }
        protected int[] BuildTriangles(int loopLen, int loopIndex, int vertIndex)
        {
            int nxt1 = (loopLen * loopIndex) + (vertIndex + 1);
            if (nxt1 >= loopLen * (loopIndex + 1))
                nxt1 = loopLen * loopIndex;

            int nxt2 = (loopLen * (loopIndex - 1)) + (vertIndex + 1);
            if (nxt2 >= loopLen * loopIndex)
                nxt2 = loopLen * (loopIndex - 1);

            //Clockwise!!!
            int[] tris = new int[] { (loopLen * loopIndex) + vertIndex, nxt1, nxt2,
                                             (loopLen * loopIndex) + vertIndex, nxt2, (loopLen * (loopIndex - 1)) + vertIndex };

            /*  //Counter-clockwise
            int prev = (loopLen * i) + (j - 1);

            if (prev == -1)
                prev = loopLen - 1;

            int[] tris = new int[] { (loopLen * i) + j, prev                   , (loopLen * (i - 1)) + j,
                                     (loopLen * i) + j, (loopLen * (i - 1)) + j, (loopLen * (i - 1)) + (j + 1) };
            */

            return tris;
        }
        protected float GetRoundingAmount(int[][] roundingRanges, int roundingRangeI, int roundingRangeJ)
        {
            float roundingAmount = 0f;
            if (roundingRangeJ > roundingRanges[roundingRangeI].Length / 2)
            {
                roundingAmount = 1f - Mathf.InverseLerp(roundingRanges[roundingRangeI].Length / 2, roundingRanges[roundingRangeI].Length, roundingRangeJ);
            }
            else
            {
                roundingAmount = Mathf.InverseLerp(-1, roundingRanges[roundingRangeI].Length / 2, roundingRangeJ);
            }
            return roundingAmount;
        }
        protected Vector3[] Redeform(Vector3[] deforms, int targetLength, out int[][] roundingRanges)
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
            roundingRanges = new int[0][];
            if (factorDiff < 0)
            {
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
                int index = 0;
                int interval = (int)Math.Pow(2, factorDiff);
                int[] keyVerts = new int[deforms.Length];
                roundingRanges = new int[deforms.Length][];
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
                    int roundingCounter = 0;
                    roundingRanges[i] = new int[interval - 1];
                    if (nextKV == keyVerts.Length)
                    {
                        nextKV = 0;
                        for (int j = 1; j < keyVerts[1]; j++)
                        {
                            roundingRanges[i][roundingCounter] = keyVerts[i] + j;
                            newD[keyVerts[i] + j] = new Vector3(
                                Mathf.Lerp(newD[keyVerts[i]].x, newD[keyVerts[nextKV]].x, (1f / interval) * (j)),
                                Mathf.Lerp(newD[keyVerts[i]].y, newD[keyVerts[nextKV]].y, (1f / interval) * (j)),
                                Mathf.Lerp(newD[keyVerts[i]].z, newD[keyVerts[nextKV]].z, (1f / interval) * (j)));
                            roundingCounter++;
                        }
                    }
                    else
                    {
                        for (int j = keyVerts[i] + 1; j < keyVerts[nextKV]; j++)
                        {
                            roundingRanges[i][roundingCounter] = j;
                            newD[j] = new Vector3(
                                Mathf.Lerp(newD[keyVerts[i]].x, newD[keyVerts[nextKV]].x, (1f / interval) * (j - keyVerts[i])),
                                Mathf.Lerp(newD[keyVerts[i]].y, newD[keyVerts[nextKV]].y, (1f / interval) * (j - keyVerts[i])),
                                Mathf.Lerp(newD[keyVerts[i]].z, newD[keyVerts[nextKV]].z, (1f / interval) * (j - keyVerts[i])));
                            roundingCounter++;
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
        public override void Build(int subdiv, float rounding, Vector3 size, int nLoops, Vector3[] deforms, SplineParams sParams)
        {
            mesh = new Mesh();
            int[][] roundingRanges = new int[0][];
            List<Vector3> allVerts = new List<Vector3>();
            int loopLen = 4 * (int)Math.Pow(2, subdiv);
            nLoops += 2;

            //Ensure deforms matches looplen
            if(deforms.Length != loopLen)
            {
                deforms = Redeform(deforms, loopLen, out roundingRanges);
            }

            //Create Verts
            for (int i = 0; i < nLoops; i++)
            {
                allVerts.AddRange(BuildLoop(loopLen, nLoops, i, size, deforms));
            }

            //Create Tris
            List<int> allTris = new List<int>();
            for (int i = 1; i < nLoops; i++)
            {
                for (int j = 0; j < loopLen; j++)
                {
                    allTris.AddRange(BuildTriangles(loopLen, i, j));
                }
            }


            mesh.vertices  = allVerts.ToArray();
            mesh.triangles = allTris.ToArray();
            mesh.RecalculateNormals();

            //Apply rounding
            for (int i = 0; i < nLoops; i++)
            {
                for (int j = 0; j < roundingRanges.Length; j++)
                {
                    for (int k = 0; k < roundingRanges[j].Length; k++)
                    {
                        float roundingAmount = GetRoundingAmount(roundingRanges, j, k);
                        allVerts[roundingRanges[j][k] + (i * loopLen)] +=
                        mesh.normals[roundingRanges[j][k] +
                        (i * loopLen)].normalized *
                        Mathf.Lerp(
                            0,
                            rounding,
                            roundingAmount + (rounding * .3f)
                        );
                    }
                }
            }

            mesh.vertices = allVerts.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
    }

    [Serializable]
    public class SUnion : SElement
    {

        public override void Build(int subdiv, float rounding, Vector3 size, int nLoops, Vector3[] deforms, SplineParams sParams)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class STerminal : SElement
    {
        public override void Build(int subdiv, float rounding, Vector3 size, int nLoops, Vector3[] deforms, SplineParams sParams)
        {
            mesh = new Mesh();
            int[][] roundingRanges = new int[0][];
            List<Vector3> allVerts = new List<Vector3>();
            int loopLen = 4 * (int)Math.Pow(2, subdiv);
            nLoops += 2;

            //Ensure deforms matches looplen
            if (deforms.Length != loopLen)
            {
                deforms = Redeform(deforms, loopLen, out roundingRanges);
            }

            //Create Verts
            for (int i = 0; i < nLoops; i++)
            {
                //Taper
                Vector3 sizeAdjustedForTaper = size;
                float taperScalar = 1f - Mathf.InverseLerp(0, nLoops-1, i);
                sizeAdjustedForTaper.x *= taperScalar;
                sizeAdjustedForTaper.z *= taperScalar;

                Vector3[] newLoop = BuildLoop(loopLen, nLoops, i, sizeAdjustedForTaper, deforms);
                allVerts.AddRange(newLoop);

                if(i == nLoops - 1)
                {
                    //Add end-cap vert
                    Vector3 endCapVert = new Vector3();
                    for (int j = 0; j < newLoop.Length; j++)
                    {
                        endCapVert += newLoop[j];
                    }
                    endCapVert /= newLoop.Length;
                    allVerts.Add(endCapVert);
                }
            }

            //Create Tris
            List<int> allTris = new List<int>();
            for (int i = 1; i < nLoops; i++)
            {
                for (int j = 0; j < loopLen; j++)
                {
                    allTris.AddRange(BuildTriangles(loopLen, i, j));
                }
            }


            mesh.vertices = allVerts.ToArray();
            mesh.triangles = allTris.ToArray();
            mesh.RecalculateNormals();

            //Apply rounding
            for (int i = 0; i < nLoops; i++)
            {
                for (int j = 0; j < roundingRanges.Length; j++)
                {
                    for (int k = 0; k < roundingRanges[j].Length; k++)
                    {
                        float roundingAmount = GetRoundingAmount(roundingRanges, j, k);
                        allVerts[roundingRanges[j][k] + (i * loopLen)] +=
                        mesh.normals[roundingRanges[j][k] +
                        (i * loopLen)].normalized *
                        Mathf.Lerp(
                            0,
                            rounding,
                            roundingAmount + (rounding * .3f)
                        );
                    }
                }
            }


            //End-cap triangles
            int refPoint = nLoops - 1;
            int[] endCap = new int[loopLen * 3];
            for (int j = 0; j < endCap.Length / 3; j++)
            {
                endCap[j * 3] = (refPoint * loopLen) + j;
                endCap[(j * 3) + 1] = (refPoint + 1) * loopLen;
                if (j == loopLen - 1)
                    endCap[(j * 3) + 2] = refPoint * loopLen;
                else
                    endCap[(j * 3) + 2] = (refPoint * loopLen) + j + 1;
            }
            allTris.AddRange(endCap);

            mesh.vertices = allVerts.ToArray();
            mesh.triangles = allTris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
    }
}
