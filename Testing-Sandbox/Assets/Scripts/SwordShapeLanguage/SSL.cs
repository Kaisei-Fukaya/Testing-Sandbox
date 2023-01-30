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
            Stack<int> edgeStack = new Stack<int>();
            int currentNodeIndex = 0;
            int currentFace = 0;
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

                    SElement[] currentStackState = stack.ToArray();
                    int[] currentEdgeStackState = edgeStack.ToArray();
                    Matrix4x4 transformationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                    for (int i = currentStackState.Length - 2; i >= 0; i--)
                    {
                        transformationMatrix *= currentStackState[i].GetConnectionData(currentEdgeStackState[i]).transformationMatrix;
                    }

                    newMesh = JoinMeshes(newMesh, subMesh, _elementSpacing, _subdiv, currentNode.GetSubmeshIndex(), currentFace, transformationMatrix);
                    completionLookup[currentNode] = true;
                }
                
                bool nextFound = false;
                for (int i = 0; i < currentNodeConnections.Length; i++)
                {
                    if (currentNodeConnections[i] == -1)
                        continue;
                    if (!completionLookup[_nodes[currentNodeConnections[i]]])
                    {
                        currentNodeIndex = currentNodeConnections[i];
                        currentFace = i;
                        edgeStack.Push(i);
                        stack.Push(_nodes[currentNodeIndex]);
                        nextFound = true;
                        break;
                    }
                }

                if (!nextFound) 
                {
                    stack.Pop();
                    if(edgeStack.Count > 0)
                        edgeStack.Pop();
                }
            }
            newMesh.RecalculateNormals();
            return newMesh;
        }

        Mesh JoinMeshes(Mesh meshA, Mesh meshB, float elementSpacing, int subdiv, int submeshIndex, int currentFace, Matrix4x4 transformationMatrix)
        {
            int vertCount = meshA.vertexCount;

            if (vertCount == 0)
            {
                return meshB;
            }

            //Merge into one mesh and reposition mb

            Mesh outMesh = new Mesh();

            int loopLen = 4 * (int)Math.Pow(2, subdiv);

            Vector3[] lastLoopOfMeshB = new Vector3[loopLen];
            for (int i = 0; i < loopLen; i++)
            {
                lastLoopOfMeshB[i] = meshA.vertices[meshA.vertices.Length - (loopLen - i)];
            }

            List<Vector3> newVerts = new List<Vector3>(meshA.vertices);
            Vector3[] bVerts = meshB.vertices;
            for (int i = 0; i < bVerts.Length; i++)
            {
                //bVerts[i].x += connectionData.position.x;
                //bVerts[i].y += connectionData.position.y + elementSpacing;
                //bVerts[i].z += connectionData.position.z;
                bVerts[i] = transformationMatrix.MultiplyPoint3x4(bVerts[i]);
            }
            int[] bTriangs = meshB.triangles;
            for (int i = 0; i < bTriangs.Length; i++)
            {
                bTriangs[i] += vertCount;
            }
            newVerts.AddRange(bVerts);
            outMesh.vertices = newVerts.ToArray();

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
            //var subMeshDesc = new UnityEngine.Rendering.SubMeshDescriptor(){firstVertex = meshA.vertexCount,
            //                                                                vertexCount = newVerts.Count - meshA.vertexCount - 1};
            //outMesh.SetSubMesh(submeshIndex, subMeshDesc);

            //Debug.Log(newMesh.vertexCount);
            return outMesh;
        }

    }

    [Serializable]
    public struct SequentialNodeParams
    {
        [Min(0)]
        public int nLoops;
        [Min(0f)]
        public float rounding;
        public Vector3 size;
        [Range(0f,1f)]
        public float relativeForwardTaper;
        [Range(0f,1f)]
        public float relativeBackwardTaper;
        public Vector3[] deforms;
        public BezierParams curveParams;
        public int subMeshIndex;
        [HideInInspector]
        public SequentialNodeType sequentialNodeType;
        public SequentialNodeParams(int loops, float rounding, Vector3 size, float relForwardTaper, float relBackwardTaper, Vector3[] deformations, BezierParams splineParams, int subMeshIndex, SequentialNodeType sequentialNodeType)
        {
            this.nLoops = loops;
            this.rounding = rounding;
            this.size = size;
            this.relativeForwardTaper = relForwardTaper;
            this.relativeBackwardTaper = relBackwardTaper;
            this.deforms = deformations;
            this.curveParams = splineParams;
            this.subMeshIndex = subMeshIndex;
            this.sequentialNodeType = sequentialNodeType;
        }

        public static SequentialNodeParams defaultParams
        {
            get
            {
                return new SequentialNodeParams(0, 0f, new Vector3(1f,1f,1f), 1f, 1f, new Vector3[8], new BezierParams(), 0, SequentialNodeType.Middle);
            }
        }
    }

    [Serializable]
    public struct BezierParams
    {
        public Vector2 controlPoint;
        public Vector2 tipOffset;
    }

    [Serializable]
    public enum SequentialNodeType
    {
        Start,
        Middle,
        End
    }

    [Serializable]
    public abstract class SElement
    {
        [SerializeField] SequentialNodeParams storedParameters;
        protected Mesh mesh;
        public Mesh GetMesh() => mesh;
        protected FacePlanarNormals facePlanarNormals;
        public int GetSubmeshIndex() => storedParameters.subMeshIndex;
        public ConnectionData GetConnectionData(int face)
        {
            ConnectionData newConnectionData = new ConnectionData();
            switch (face)
            {
                case 0:
                    newConnectionData = new ConnectionData(facePlanarNormals.t_centre, Quaternion.LookRotation(Vector3.forward, facePlanarNormals.t));
                    break;
                case 1:
                    newConnectionData = new ConnectionData(facePlanarNormals.l_centre, Quaternion.LookRotation(Vector3.forward, facePlanarNormals.l));
                    break;
                case 2:
                    newConnectionData = new ConnectionData(facePlanarNormals.f_centre, Quaternion.LookRotation(Vector3.right, facePlanarNormals.f));
                    break;
                case 3:
                    newConnectionData = new ConnectionData(facePlanarNormals.r_centre, Quaternion.LookRotation(Vector3.back, facePlanarNormals.r));
                    break;
                case 4:
                    newConnectionData = new ConnectionData(facePlanarNormals.ba_centre, Quaternion.LookRotation(Vector3.left, facePlanarNormals.ba));
                    break;
            }

            return newConnectionData;
        }
        public struct ConnectionData
        {
            public Vector3 position;
            public Quaternion direction;
            public Matrix4x4 transformationMatrix
            {
                get
                {
                    return Matrix4x4.TRS(position, direction, Vector3.one);
                }
            }
            public ConnectionData(Vector3 position, Quaternion direction)
            {
                this.position = position;
                this.direction = direction;
            }
        }
        public virtual void Build(int subdivs)
        {
            Build(subdivs, storedParameters);
        }
        public void Build(int subdiv, SequentialNodeParams parameters)
        {
            storedParameters = parameters;
            Build(subdiv, parameters.rounding, parameters.size, parameters.relativeForwardTaper, parameters.relativeBackwardTaper, parameters.nLoops, parameters.deforms, parameters.curveParams, parameters.sequentialNodeType);
        }
        public abstract void Build(int subdiv, float rounding, Vector3 size, float relativeForwardTaper, float relativeBackwardTaper, int nLoops, Vector3[] deforms, BezierParams sParams, SequentialNodeType sequentialNodeType);
        protected Vector3[] BuildLoop(int loopLen, int nLoops, int index, Vector3 size, Vector3[] deforms, float taperScale)
        {
            Vector3[] verts = new Vector3[loopLen];

            //Determine loop curve offset (excl first and last loop)
            Vector3 curveOffset;
            if (index == 0)
                curveOffset = Vector3.zero;
            else if (index == nLoops - 1)
            {
                Vector2 tipOffset = TipOffset;
                curveOffset = new Vector3(tipOffset.x, size.y, tipOffset.y);
            }
            else
                curveOffset = SampleQuadraticBezier(Mathf.InverseLerp(0, nLoops-1, index));

            float yOffset = (size.y / (nLoops - 1)) * index;
            int quartOfLength = verts.Length / 4;

            verts[0] =                 new Vector3(curveOffset.x + (-size.x / 2),
                                                   curveOffset.y,
                                                   curveOffset.z + (-size.z / 2)) + (deforms[0] * taperScale);
            verts[quartOfLength] =     new Vector3(curveOffset.x + (size.x / 2),
                                                   curveOffset.y,
                                                   curveOffset.z + (-size.z / 2)) + (deforms[quartOfLength] * taperScale);
            verts[quartOfLength * 2] = new Vector3(curveOffset.x + (size.x / 2),
                                                   curveOffset.y,
                                                   curveOffset.z + (size.z / 2)) + (deforms[quartOfLength * 2] * taperScale);
            verts[quartOfLength * 3] = new Vector3(curveOffset.x + (-size.x / 2),
                                                   curveOffset.y,
                                                   curveOffset.z + (size.z / 2)) + (deforms[quartOfLength * 3] * taperScale);


            //Debug.Log($"vertsL= {verts.Length}");
            //Between 0-1
            for (int j = 1; j < quartOfLength; j++)
            {
                Vector3 baseOffset = new Vector3(
                    curveOffset.x + (-size.x / 2) + ((size.x / quartOfLength) * j),
                    curveOffset.y,
                    curveOffset.z + (-size.z / 2)
                    );
                verts[j] = baseOffset + (deforms[j] * taperScale);
            //Debug.Log($"j1= {j}");
        }
            //Between 1-2
            for (int j = quartOfLength + 1; j < quartOfLength * 2; j++)
            {
                Vector3 baseOffset = new Vector3(
                   curveOffset.x + size.x / 2,
                   curveOffset.y,
                   curveOffset.z + (-size.z / 2) + ((size.z / quartOfLength) * (j - quartOfLength))
                   );
                verts[j] = baseOffset + (deforms[j] * taperScale);
            //Debug.Log($"j2= {j}");
        }
            //Between 2-3
            for (int j = (quartOfLength * 2) + 1; j < quartOfLength * 3; j++)
            {
                Vector3 baseOffset = new Vector3(
                    curveOffset.x + (size.x / 2) - ((size.x / quartOfLength) * (j - (quartOfLength * 2))),
                    curveOffset.y,
                    curveOffset.z + (size.z / 2)
                    );
                verts[j] = baseOffset + (deforms[j] * taperScale);
            //Debug.Log($"j3= {j}");
        }
            //Between 3-0
            for (int j = (quartOfLength * 3) + 1; j < verts.Length; j++)
            {
                Vector3 baseOffset = new Vector3(
                    curveOffset.x + -size.x / 2,
                    curveOffset.y,
                    curveOffset.z + (size.z / 2) - ((size.z / quartOfLength) * (j - (quartOfLength * 3)))
                    );
                verts[j] = baseOffset + (deforms[j] * taperScale);
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
        protected Vector3 SampleQuadraticBezier(float t)
        {
            Vector3 origin = Vector3.zero;
            Vector3 control = new Vector3(storedParameters.curveParams.controlPoint.x, storedParameters.size.y/2, storedParameters.curveParams.controlPoint.y);
            Vector3 end = new Vector3(storedParameters.curveParams.tipOffset.x, 
                                      storedParameters.size.y, 
                                      storedParameters.curveParams.tipOffset.y);
            Vector3 p0 = Vector3.Lerp(origin, control, t);
            Vector3 p1 = Vector3.Lerp(control, end, t);
            return Vector3.Lerp(p0, p1, t);
        }
        protected FacePlanarNormals GenerateFacePlanarNormals(int nLoops, Vector3[] verts, int loopLen)
        {
            int quartOfLength = loopLen / 4;
            var a  = verts[0];
            var b  = verts[quartOfLength];
            var c  = verts[quartOfLength * 2];
            var d  = verts[quartOfLength * 3];
            var a1 = verts[(nLoops - 1) * loopLen];
            var b1 = verts[((nLoops - 1) * loopLen) + quartOfLength];
            var c1 = verts[((nLoops - 1) * loopLen) + (quartOfLength * 2)];
            var d1 = verts[((nLoops - 1) * loopLen) + (quartOfLength * 3)];
        //        bottom: Vector3.Cross(b - d, a - d).normalized,
        //        top: Vector3.Cross(c1 - d1, b1 - d1).normalized,
        //        left: Vector3.Cross(d - a1, a - a1).normalized,
        //        front: Vector3.Cross(c - d1, d - d1).normalized,
        //        right: Vector3.Cross(b - c1, c - c1).normalized,
        //        back: Vector3.Cross(a - b1, b - b1).normalized,
            facePlanarNormals = new FacePlanarNormals(
                bottom: Vector3.Cross(d-a+c-b, b-a+c-d).normalized,
                top:    Vector3.Cross(a1-d1+b1-c1, a1-b1+d1-c1).normalized,
                left:   Vector3.Cross(a-d+a1-d1, a-a1+d-d1).normalized,
                front:  Vector3.Cross(d-c+d1-c1, d-d1+c-c1).normalized,
                right:  Vector3.Cross(c-b+c1-b1, c-c1+b-b1).normalized,
                back:   Vector3.Cross(b-a+b1-a1, b-b1+a-a1).normalized,
                bottom_centre: (a+b+c+d)/4,
                top_centre:    (a1+b1+c1+d1)/4,
                left_centre:   (a+a1+d+d1)/4,
                front_centre:  (c+c1+d+d1)/4,
                right_centre:  (b+b1+c+c1)/4,
                back_centre:   (a+a1+b+b1)/4
                );
            return facePlanarNormals;
        }
        public struct FacePlanarNormals
        {
            public Vector3 bo;
            public Vector3 t;
            public Vector3 l;
            public Vector3 f;
            public Vector3 r;
            public Vector3 ba;
            public Vector3 bo_centre;
            public Vector3 t_centre;
            public Vector3 l_centre;
            public Vector3 f_centre;
            public Vector3 r_centre;
            public Vector3 ba_centre;
            public FacePlanarNormals(Vector3 bottom, Vector3 top, Vector3 left, Vector3 front, Vector3 right, Vector3 back,
                                     Vector3 bottom_centre, Vector3 top_centre, Vector3 left_centre, Vector3 front_centre, Vector3 right_centre, Vector3 back_centre)
            {
                bo = bottom;
                t = top;
                l = left;
                f = front;
                r = right;
                ba = back;
                bo_centre = bottom_centre;
                t_centre = top_centre;
                l_centre = left_centre;
                f_centre = front_centre;
                r_centre = right_centre;
                ba_centre = back_centre;
            }

        }
        protected Vector2 TipOffset => storedParameters.curveParams.tipOffset;

        protected float GetTaperScale(Vector3 size, int nLoops, int loopIndex, float relativeForwardTaper, float relativeBackwardTaper)
        {
            if (loopIndex > nLoops / 2)
                return Mathf.Clamp((Mathf.InverseLerp(nLoops - 1, nLoops / 2, loopIndex) + relativeForwardTaper), 0f, 1f);
            else if (loopIndex < nLoops / 2)
                return Mathf.Clamp((Mathf.InverseLerp(0, nLoops / 2, loopIndex) + relativeBackwardTaper), 0f, 1f);
            else
                return 1f;
        }
    }

    [Serializable]
    public class SSegment : SElement
    {
        public override void Build(int subdiv, float rounding, Vector3 size, float relativeForwardTaper, float relativeBackwardTaper, int nLoops, Vector3[] deforms, BezierParams sParams, SequentialNodeType sequentialNodeType)
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
                float taperScale = GetTaperScale(size, nLoops, i, relativeForwardTaper, relativeBackwardTaper);
                Vector3 sizeAdjustedForTaper = size;
                sizeAdjustedForTaper.x *= taperScale;
                sizeAdjustedForTaper.z *= taperScale;

                Vector3[] newLoop = BuildLoop(loopLen, nLoops, i, sizeAdjustedForTaper, deforms, taperScale);
                allVerts.AddRange(newLoop);

                //Add end-cap vert
                if (sequentialNodeType == SequentialNodeType.End && i == nLoops - 1)
                {
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

            GenerateFacePlanarNormals(nLoops, allVerts.ToArray(), loopLen);

            if (sequentialNodeType == SequentialNodeType.Start)
            {
                //Start-cap vert
                Vector3 startCapVert = new Vector3();
                for (int j = 0; j < loopLen; j++)
                {
                    startCapVert += allVerts[j];
                }
                startCapVert /= loopLen;
                allVerts.Insert(0, startCapVert);

                //Shift tris up to account for start cap vert
                for (int i = 0; i < allTris.Count; i++)
                {
                    allTris[i]++;
                }

                //Start-cap triangles
                int[] startCap = new int[loopLen * 3];
                for (int i = 0; i < startCap.Length / 3; i++)
                {
                    if (i == loopLen - 1)
                    {
                        startCap[i * 3] = 0;
                        startCap[(i * 3) + 1] = i + 1;
                        startCap[(i * 3) + 2] = 1;
                    }
                    else
                    {
                        startCap[i * 3] = 0;
                        startCap[(i * 3) + 1] = i + 1;
                        startCap[(i * 3) + 2] = i + 2;
                    }
                }
                allTris.AddRange(startCap);
            }
            //End-cap triangles
            else if (sequentialNodeType == SequentialNodeType.End)
            {
                int refPoint = nLoops - 1;
                int[] endCap = new int[loopLen * 3];
                for (int i = 0; i < endCap.Length / 3; i++)
                {
                    endCap[i * 3] = (refPoint * loopLen) + i;
                    endCap[(i * 3) + 1] = (refPoint + 1) * loopLen;
                    if (i == loopLen - 1)
                        endCap[(i * 3) + 2] = refPoint * loopLen;
                    else
                        endCap[(i * 3) + 2] = (refPoint * loopLen) + i + 1;
                }
                allTris.AddRange(endCap);
            }

            mesh.vertices = allVerts.ToArray();
            mesh.triangles = allTris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
    }

    [Serializable]
    public class SBranch : SElement
    {

        public override void Build(int subdiv, float rounding, Vector3 size, float relativeForwardTaper, float relativeBackwardTaper, int nLoops, Vector3[] deforms, BezierParams sParams, SequentialNodeType sequentialNodeType)
        {
            mesh = new Mesh();
            int[][] roundingRanges = new int[0][];
            List<Vector3> allVerts = new List<Vector3>();
            int loopLen = 4 * (int)Math.Pow(2, subdiv);
            nLoops = (loopLen / 4) + 1;

            //Ensure deforms matches looplen
            if (deforms.Length != loopLen)
            {
                deforms = Redeform(deforms, loopLen, out roundingRanges);
            }

            //Create Verts
            for (int i = 0; i < nLoops; i++)
            {
                //Taper
                //float taperScale = GetTaperScale(size, nLoops, i, relativeForwardTaper, relativeBackwardTaper);
                //Vector3 sizeAdjustedForTaper = size;
                //sizeAdjustedForTaper.x *= taperScale;
                //sizeAdjustedForTaper.z *= taperScale;

                Vector3[] newLoop = BuildLoop(loopLen, nLoops, i, size, deforms, 1f);
                allVerts.AddRange(newLoop);

                //Add end-cap vert
                if (sequentialNodeType == SequentialNodeType.End && i == nLoops - 1)
                {
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

            GenerateFacePlanarNormals(nLoops, allVerts.ToArray(), loopLen);

            if (sequentialNodeType == SequentialNodeType.Start)
            {
                //Start-cap vert
                Vector3 startCapVert = new Vector3();
                for (int j = 0; j < loopLen; j++)
                {
                    startCapVert += allVerts[j];
                }
                startCapVert /= loopLen;
                allVerts.Insert(0, startCapVert);

                //Shift tris up to account for start cap vert
                for (int i = 0; i < allTris.Count; i++)
                {
                    allTris[i]++;
                }

                //Start-cap triangles
                int[] startCap = new int[loopLen * 3];
                for (int i = 0; i < startCap.Length / 3; i++)
                {
                    if (i == loopLen - 1)
                    {
                        startCap[i * 3] = 0;
                        startCap[(i * 3) + 1] = i + 1;
                        startCap[(i * 3) + 2] = 1;
                    }
                    else
                    {
                        startCap[i * 3] = 0;
                        startCap[(i * 3) + 1] = i + 1;
                        startCap[(i * 3) + 2] = i + 2;
                    }
                }
                allTris.AddRange(startCap);
            }
            //End-cap triangles
            else if (sequentialNodeType == SequentialNodeType.End)
            {
                int refPoint = nLoops - 1;
                int[] endCap = new int[loopLen * 3];
                for (int i = 0; i < endCap.Length / 3; i++)
                {
                    endCap[i * 3] = (refPoint * loopLen) + i;
                    endCap[(i * 3) + 1] = (refPoint + 1) * loopLen;
                    if (i == loopLen - 1)
                        endCap[(i * 3) + 2] = refPoint * loopLen;
                    else
                        endCap[(i * 3) + 2] = (refPoint * loopLen) + i + 1;
                }
                allTris.AddRange(endCap);
            }

            mesh.vertices = allVerts.ToArray();
            mesh.triangles = allTris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
    }

    [Serializable]
    public class STerminal : SElement
    {
        public override void Build(int subdiv, float rounding, Vector3 size, float relativeForwardTaper, float relativeBackwardTaper, int nLoops, Vector3[] deforms, BezierParams sParams, SequentialNodeType sequentialNodeType)
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
                float taperScale = GetTaperScale(size, nLoops, i, relativeForwardTaper, relativeBackwardTaper);
                Vector3 sizeAdjustedForTaper = size;
                sizeAdjustedForTaper.x *= taperScale;
                sizeAdjustedForTaper.z *= taperScale;

                Vector3[] newLoop = BuildLoop(loopLen, nLoops, i, sizeAdjustedForTaper, deforms, taperScale);
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
