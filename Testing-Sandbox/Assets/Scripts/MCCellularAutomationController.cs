using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCCellularAutomationController : MarchingCubesController
{
    PointGrid _pGridNew;
    Coroutine _automation;
    public float outerPointValue;
    [Min(0)]
    public int iterations;

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
    }

    public void StartAutomation()
    {
        //Initialise
        StopAllCoroutines();
        GenerateRand();

        //Begin Automation
        _automation = StartCoroutine(Automation());
    }

    IEnumerator Automation()
    {
        for (int i = 0; i < iterations; i++)
        {
            AffectCells();
            Generate(_pGrid);
            print(i);
            yield return null;
        }
    }

    void AffectCells()
    {
        _pGridNew = _pGrid;
        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    //Game of life
                    int neighbourCount = GetNeighbourCount(i, j, k);

                    //If cell is alive
                    if (_pGrid.points[i, j, k].w == 1f)
                    {
                        //Death (underpop)
                        if (neighbourCount < 2)
                        {
                            _pGridNew.points[i, j, k].w = 0f;
                        }

                        //Live
                        if (neighbourCount == 2 || neighbourCount == 3)
                        {
                            _pGridNew.points[i, j, k].w = 1f;
                        }

                        //Death (overpop)
                        if (neighbourCount > 3)
                        {
                            _pGridNew.points[i, j, k].w = 0f;
                        }
                    }
                    else
                    {
                        //Live (repop)
                        if (neighbourCount == 3)
                        {
                            _pGridNew.points[i, j, k].w = 1f;
                        }
                    }
                }
            }
        }
        _pGrid = _pGridNew;
    }

    int GetNeighbourCount(int pX, int pY, int pZ)
    {
        int neighbourCount = 0;

        for (int i = pX - 1; i < pX + 1; i++)
        {
            for (int j = pY - 1; j < pY + 1; j++)
            {
                for (int k = pZ - 1; k < pZ + 1; k++)
                {
                    //Ensure In Range
                    if ((i < _pGrid.xLength && i >= 0) &&
                        (j < _pGrid.yLength && j >= 0) &&
                        (k < _pGrid.zLength && k >= 0))
                    {
                        //Ignore cell itself
                        if (!(i == pX && j == pY && k == pZ))
                        {
                            neighbourCount += (int)_pGrid.points[i, j, k].w;
                        }
                    }
                    else
                    {
                        neighbourCount++;
                    }
                }
            }
        }

        return neighbourCount;
    }
}
