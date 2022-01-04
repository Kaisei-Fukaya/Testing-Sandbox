using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public abstract class MarchingCubesController : MonoBehaviour
{
    protected MeshFilter _meshFilter;

    [Range(0f,1f)]
    public float _isoLevel = .5f;

    public float _gridSpacing = 1f;
    public int _gridSize = 10;

    public bool _visualisePoints = false;


    protected PointGrid _pGrid;
    protected MarchingCubes _mc;

    public virtual void Generate(PointGrid pGrid)
    {
        if (_mc == null) _mc = new MarchingCubes();
        if (_meshFilter == null) { _meshFilter = GetComponent<MeshFilter>(); }
        Mesh mesh = _mc.March(pGrid, _isoLevel);
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
    }

    protected void GenerateMesh(Vector3[] vertices, int[] tris)
    {
        if (_meshFilter == null){ _meshFilter = GetComponent<MeshFilter>(); }
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = tris;
        _meshFilter.mesh = newMesh;
    }

    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += DelayedOnValidate;
    }

    protected virtual void DelayedOnValidate()
    {
        UnityEditor.EditorApplication.delayCall -= DelayedOnValidate;
        if (this == null) return;
    }

    protected void OnDrawGizmos()
    {
        //Visualise Bounds
        Vector3 size = new Vector3(_gridSpacing * (_gridSize - 1), _gridSpacing * (_gridSize - 1), _gridSpacing * (_gridSize - 1));
        Vector3 centre = new Vector3(transform.position.x + (size.x / 2), transform.position.y + (size.y / 2), transform.position.z + (size.z / 2));
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(centre, size);

        //Visualise Points
        if (!_pGrid.IsInitialised() || !_visualisePoints) { return; }
        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    Vector3 offset = _pGrid.points[i,j,k];
                    Color newColour = Color.white;
                    newColour.a = _pGrid.points[i, j, k].w;
                    Gizmos.color = newColour;
                    Gizmos.DrawSphere(offset, .1f);
                }
            }
        }
    }

}
