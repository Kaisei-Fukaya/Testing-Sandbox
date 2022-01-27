using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class MarchingCubesPaintController : MarchingCubesController
{
    [Min(0)]
    public int baseGroundHeight = 0;

    public int brushSize = 1;
    public float brushStrength = .1f;

    Camera _mainCam;
    MeshCollider _meshCollider;

    bool _userIsClicking = false, _userIsHoldingInverter = false;


    public void Start()
    {
        GeneratePlane();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _userIsClicking = true;
            if (Input.GetButton("Inverter"))
            {
                _userIsHoldingInverter = true;
            }
            else
            {
                _userIsHoldingInverter = false;
            }
            return;
        }
        _userIsClicking = false;
        _userIsHoldingInverter = false;
    }

    private void FixedUpdate()
    {
        if (_userIsClicking)
        {
            if (_userIsHoldingInverter)
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
        _pGrid = new PointGrid(gridSize, gridSize, gridSize);
        if(baseGroundHeight > _pGrid.yLength) { return; }

        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    _pGrid.points[i, j, k].x = i * gridSpacing;
                    _pGrid.points[i, j, k].y = j * gridSpacing;
                    _pGrid.points[i, j, k].z = k * gridSpacing;

                    if (j > baseGroundHeight)
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
                EditValueAtCoord(pX, pY, pZ, brushStrength, false);
                for (int i = 1; i < brushSize; i++)
                {
                    //Orthogonally adjacent
                    EditValueAtCoord(pX + i, pY, pZ, brushStrength * (.8f / i), false);
                    EditValueAtCoord(pX, pY + i, pZ, brushStrength * (.8f / i), false);
                    EditValueAtCoord(pX, pY, pZ + i, brushStrength * (.8f / i), false);
                    EditValueAtCoord(pX - i, pY, pZ, brushStrength * (.8f / i), false);
                    EditValueAtCoord(pX, pY - i, pZ, brushStrength * (.8f / i), false);
                    EditValueAtCoord(pX, pY, pZ - i, brushStrength * (.8f / i), false);

                    //X Diags
                    EditValueAtCoord(pX - i, pY + i, pZ, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX + i, pY + i, pZ, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX - i, pY - i, pZ, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX + i, pY - i, pZ, brushStrength * (.7f / i), false);
                                                                             
                    //Y Diags                                                
                    EditValueAtCoord(pX - i, pY, pZ + i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX + i, pY, pZ + i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX - i, pY, pZ - i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX + i, pY, pZ - i, brushStrength * (.7f / i), false);
                                                                             
                    //Z Diags                                                
                    EditValueAtCoord(pX, pY + i, pZ - i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX, pY + i, pZ + i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX, pY - i, pZ - i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX, pY - i, pZ + i, brushStrength * (.7f / i), false);

                    //Corners
                    EditValueAtCoord(pX + i, pY + i, pZ + i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX + i, pY + i, pZ - i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX + i, pY - i, pZ + i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX + i, pY - i, pZ - i, brushStrength * (.7f / i), false);
                                                                                 
                    EditValueAtCoord(pX - i, pY + i, pZ + i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX - i, pY + i, pZ - i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX - i, pY - i, pZ + i, brushStrength * (.7f / i), false);
                    EditValueAtCoord(pX - i, pY - i, pZ - i, brushStrength * (.7f / i), false);
                }
            }
            else 
            {
                EditValueAtCoord(pX, pY, pZ, brushStrength, true);
                for (int i = 1; i < brushSize; i++)
                {
                    //Orthogonally adjacent
                    EditValueAtCoord(pX + i, pY, pZ, brushStrength * (.8f / i), true);
                    EditValueAtCoord(pX, pY + i, pZ, brushStrength * (.8f / i), true);
                    EditValueAtCoord(pX, pY, pZ + i, brushStrength * (.8f / i), true);
                    EditValueAtCoord(pX - i, pY, pZ, brushStrength * (.8f / i), true);
                    EditValueAtCoord(pX, pY - i, pZ, brushStrength * (.8f / i), true);
                    EditValueAtCoord(pX, pY, pZ - i, brushStrength * (.8f / i), true);

                    //X Diags
                    EditValueAtCoord(pX - i, pY + i, pZ, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX + i, pY + i, pZ, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX - i, pY - i, pZ, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX + i, pY - i, pZ, brushStrength * (.7f / i), true);

                    //Y Diags                                                
                    EditValueAtCoord(pX - i, pY, pZ + i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX + i, pY, pZ + i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX - i, pY, pZ - i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX + i, pY, pZ - i, brushStrength * (.7f / i), true);

                    //Z Diags                                                
                    EditValueAtCoord(pX, pY + i, pZ - i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX, pY + i, pZ + i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX, pY - i, pZ - i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX, pY - i, pZ + i, brushStrength * (.7f / i), true);

                    //Corners
                    EditValueAtCoord(pX + i, pY + i, pZ + i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX + i, pY + i, pZ - i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX + i, pY - i, pZ + i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX + i, pY - i, pZ - i, brushStrength * (.7f / i), true);

                    EditValueAtCoord(pX - i, pY + i, pZ + i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX - i, pY + i, pZ - i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX - i, pY - i, pZ + i, brushStrength * (.7f / i), true);
                    EditValueAtCoord(pX - i, pY - i, pZ - i, brushStrength * (.7f / i), true);
                }
            }
            //Clamp to 0-1
            if(_pGrid.points[pX, pY, pZ].w < 0f) { _pGrid.points[pX, pY, pZ].w = 0f; }
            if(_pGrid.points[pX, pY, pZ].w > 1f) { _pGrid.points[pX, pY, pZ].w = 1f; }

            Generate(_pGrid);
        }
    }

    void EditValueAtCoord(int x, int y, int z, float value, bool positive)
    {
        //Ensure coords are in range
        if ((x < _pGrid.xLength && x >= 0) &&
           (y < _pGrid.yLength && y >= 0) &&
           (z < _pGrid.zLength && z >= 0))
        {
            if (positive) { _pGrid.points[x, y, z].w += value; }
            else { _pGrid.points[x, y, z].w -= value; }
        }
    }


    Vector3 WorldPointToPointGridIndex(Vector3 worldPoint)
    {
        Vector3 coords = worldPoint - transform.position;
        coords.x = RoundTo(coords.x, gridSpacing);
        coords.y = RoundTo(coords.y, gridSpacing);
        coords.z = RoundTo(coords.z, gridSpacing);
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
        Mesh mesh = _mc.March(pGrid, isoLevel);
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;

        //Set up collider
        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = mesh;
    }

    protected override void DelayedOnValidate()
    {
        return;
    }
}
