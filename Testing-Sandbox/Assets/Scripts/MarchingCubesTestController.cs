using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MarchingCubesTestController : MarchingCubesController
{

    public float _perlinScale = 10f;

    public void GenerateGradient()
    {
        _pGrid = new PointGrid(_gridSize, _gridSize, _gridSize);

        float val = 1f;
        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    _pGrid.points[i, j, k].x = i * _gridSpacing;
                    _pGrid.points[i, j, k].y = j * _gridSpacing;
                    _pGrid.points[i, j, k].z = k * _gridSpacing;
                    _pGrid.points[i, j, k].w = val;
                    val *= .99f;
                }
            }
        }

        Generate(_pGrid);
    }


    public void GenerateRand()
    {
        _pGrid = new PointGrid(_gridSize, _gridSize, _gridSize);

        //pGrid.points[0, 0, 0].w = 0f;
        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    if (i == 0 || j == 0 || k == 0 || i == _pGrid.xLength - 1 || j == _pGrid.yLength - 1 || k == _pGrid.zLength - 1)
                    {
                        _pGrid.points[i, j, k].w = -1f;
                    }
                    else
                    {
                        _pGrid.points[i, j, k].x = i * _gridSpacing;
                        _pGrid.points[i, j, k].y = j * _gridSpacing;
                        _pGrid.points[i, j, k].z = k * _gridSpacing;
                        _pGrid.points[i, j, k].w = Random.value;
                    }
                }
            }
        }

        Generate(_pGrid);
    }

    public void GeneratePerlin()
    {
        _pGrid = new PointGrid(_gridSize, _gridSize, _gridSize);
        float offset = Random.value * 10;

        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    if (i == 0 || j == 0 || k == 0 || i == _pGrid.xLength - 1 || j == _pGrid.yLength - 1 || k == _pGrid.zLength - 1)
                    {
                        _pGrid.points[i, j, k].w = 1f;
                    }
                    else
                    {
                        _pGrid.points[i, j, k].x = i * _gridSpacing;
                        _pGrid.points[i, j, k].y = j * _gridSpacing;
                        _pGrid.points[i, j, k].z = k * _gridSpacing;
                        _pGrid.points[i, j, k].w = Perlin3D.Sample((_perlinScale * i) + offset, (_perlinScale * j) + offset, (_perlinScale * k) + offset);
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
