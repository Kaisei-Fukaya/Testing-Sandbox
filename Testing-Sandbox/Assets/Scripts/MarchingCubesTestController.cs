using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MarchingCubesTestController : MarchingCubesController
{

    public float perlinScale = 10f;
    public float outerPointValue = 1f;

    public void GenerateGradient()
    {
        _pGrid = new PointGrid(gridSize, gridSize, gridSize);

        float val = 1f;
        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    _pGrid.points[i, j, k].x = i * gridSpacing;
                    _pGrid.points[i, j, k].y = j * gridSpacing;
                    _pGrid.points[i, j, k].z = k * gridSpacing;
                    _pGrid.points[i, j, k].w = val;
                    val *= .99f;
                }
            }
        }

        Generate(_pGrid);
    }


    public void GenerateRand()
    {
        _pGrid = new PointGrid(gridSize, gridSize, gridSize);

        //pGrid.points[0, 0, 0].w = 0f;
        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    _pGrid.points[i, j, k].x = i * gridSpacing;
                    _pGrid.points[i, j, k].y = j * gridSpacing;
                    _pGrid.points[i, j, k].z = k * gridSpacing;
                    if (i == 0 || j == 0 || k == 0 || i == _pGrid.xLength - 1 || j == _pGrid.yLength - 1 || k == _pGrid.zLength - 1)
                    {
                        _pGrid.points[i, j, k].w = outerPointValue;
                    }
                    else
                    {
                        _pGrid.points[i, j, k].w = Random.value;
                    }
                }
            }
        }

        Generate(_pGrid);
    }

    public void GeneratePerlin()
    {
        _pGrid = new PointGrid(gridSize, gridSize, gridSize);
        float offset = Random.value * 10;

        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    _pGrid.points[i, j, k].x = i * gridSpacing;
                    _pGrid.points[i, j, k].y = j * gridSpacing;
                    _pGrid.points[i, j, k].z = k * gridSpacing;
                    if (i == 0 || j == 0 || k == 0 || i == _pGrid.xLength - 1 || j == _pGrid.yLength - 1 || k == _pGrid.zLength - 1)
                    {
                        _pGrid.points[i, j, k].w = outerPointValue;
                    }
                    else
                    {
                        _pGrid.points[i, j, k].w = Perlin3D.Sample((perlinScale * i) + offset, (perlinScale * j) + offset, (perlinScale * k) + offset);
                    }
                }
            }
        }

        Generate(_pGrid);
    }


    protected override void DelayedOnValidate()
    {
        base.DelayedOnValidate();
        if(_pGrid.IsInitialised()) Generate(_pGrid);
    }

}
