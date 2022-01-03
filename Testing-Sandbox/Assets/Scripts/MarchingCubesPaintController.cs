using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class MarchingCubesPaintController : MarchingCubesController
{
    [Min(0)]
    public int _baseGroundHeight = 0;

    public int _brushSize = 1;
    public float _brushStrength = .1f;

    Camera _mainCam;
    MeshCollider _meshCollider;


    public void Start()
    {
        GeneratePlane();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Input.GetButton("Jump"))
            {
                WhenClicking(false);
            }
            else
            {
                WhenClicking(true);
            }
        }
    }

    public void GeneratePlane()
    {
        _pGrid = new PointGrid(_gridSize, _gridSize, _gridSize);
        if(_baseGroundHeight > _pGrid.yLength) { return; }

        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    _pGrid.points[i, j, k].x = i * _gridSpacing;
                    _pGrid.points[i, j, k].y = j * _gridSpacing;
                    _pGrid.points[i, j, k].z = k * _gridSpacing;

                    if (j > _baseGroundHeight)
                    {
                        _pGrid.points[i, j, k].w = 1f;
                    }
                    else
                    {
                        _pGrid.points[i, j, k].w = 0f;
                    }
                }
            }
        }

        Generate(_pGrid);
    }

    void WhenClicking(bool positive)
    {
        if (_mainCam == null) { _mainCam = Camera.main; }
        RaycastHit hit;
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            Vector3 p = WorldPointToPointGridIndex(hit.point);
            int pX = (int)p.x;
            int pY = (int)p.y;
            int pZ = (int)p.z;
            if (positive) 
            {
                MinusValueAtCoord(pX, pY, pZ, _brushStrength);
                for (int i = 1; i < _brushSize; i++)
                {
                    MinusValueAtCoord(pX + i, pY, pZ, _brushStrength * (.8f / i));
                    MinusValueAtCoord(pX, pY + i, pZ, _brushStrength * (.8f / i));
                    MinusValueAtCoord(pX, pY, pZ + i, _brushStrength * (.8f / i));
                    MinusValueAtCoord(pX - i, pY, pZ, _brushStrength * (.8f / i));
                    MinusValueAtCoord(pX, pY - i, pZ, _brushStrength * (.8f / i));
                    MinusValueAtCoord(pX, pY, pZ - i, _brushStrength * (.8f / i));
                }
            }
            else 
            {
                AddValueAtCoord(pX, pY, pZ, _brushStrength);
                for (int i = 1; i < _brushSize; i++)
                {
                    AddValueAtCoord(pX + i, pY, pZ, _brushStrength * (.8f / i));
                    AddValueAtCoord(pX, pY + i, pZ, _brushStrength * (.8f / i));
                    AddValueAtCoord(pX, pY, pZ + i, _brushStrength * (.8f / i));
                    AddValueAtCoord(pX - i, pY, pZ, _brushStrength * (.8f / i));
                    AddValueAtCoord(pX, pY - i, pZ, _brushStrength * (.8f / i));
                    AddValueAtCoord(pX, pY, pZ - i, _brushStrength * (.8f / i));
                }
            }
            //Clamp to 0-1
            if(_pGrid.points[pX, pY, pZ].w < 0f) { _pGrid.points[pX, pY, pZ].w = 0f; }
            if(_pGrid.points[pX, pY, pZ].w > 1f) { _pGrid.points[pX, pY, pZ].w = 1f; }

            Generate(_pGrid);
        }
    }

    void AddValueAtCoord(int x, int y, int z, float value)
    {
        //Ensure coords are in range
        if((x < _pGrid.xLength && x >= 0) &&
           (y < _pGrid.yLength && y >= 0) &&
           (z < _pGrid.zLength && z >= 0))
        {
            _pGrid.points[x, y, z].w += value;
        }
    }

    void MinusValueAtCoord(int x, int y, int z, float value)
    {
        //Ensure coords are in range
        if ((x < _pGrid.xLength && x >= 0) &&
           (y < _pGrid.yLength && y >= 0) &&
           (z < _pGrid.zLength && z >= 0))
        {
            _pGrid.points[x, y, z].w -= value;
        }
    }


    Vector3 WorldPointToPointGridIndex(Vector3 worldPoint)
    {
        Vector3 coords = worldPoint - transform.position;
        coords.x = RoundTo(coords.x, _gridSpacing);
        coords.y = RoundTo(coords.y, _gridSpacing);
        coords.z = RoundTo(coords.z, _gridSpacing);
        return coords;
    }
    int RoundTo(float value, float multipleOf)
    {
        return (int)Mathf.Round(value / multipleOf);
    }

    public override void Generate(PointGrid pGrid)
    {
        if (_mc == null) _mc = new MarchingCubes();
        if (_meshFilter == null) { _meshFilter = GetComponent<MeshFilter>(); }
        if (_meshCollider == null) { _meshCollider = GetComponent<MeshCollider>(); }
        Mesh mesh = _mc.March(pGrid, _isoLevel);
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;

        //Set up collider
        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = mesh;
    }

    protected override void DelayedOnValidate()
    {
        base.DelayedOnValidate();
        if (_pGrid.IsInitialised()) GeneratePlane();
    }
}
