using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCCellularAutomationController : MarchingCubesController
{
    Coroutine _automation;
    public float outerPointValue;
    [Min(0)]
    public int iterations;
    [Range(0f,1f)]
    public float timeStep = .1f;

    [Header("Rules")]
    public int[] birth;
    public int[] survive;

    int iterTest;

    private void Start()
    {
        StartAutomation();
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
                        if(Random.value > .5f)
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
        Generate(_pGrid);
        iterTest = 0;
        yield return new WaitForSeconds(timeStep);
        for (int i = 0; i < iterations; i++)
        {
            AffectCells();
            Generate(_pGrid);
            iterTest++;
            yield return new WaitForSeconds(timeStep);
        }
    }

    void AffectCells()
    {
        PointGrid pGridNew = _pGrid;
        for (int i = 0; i < _pGrid.points.GetLength(0); i++)
        {
            for (int j = 0; j < _pGrid.points.GetLength(1); j++)
            {
                for (int k = 0; k < _pGrid.points.GetLength(2); k++)
                {
                    if (!(i == 0 || j == 0 || k == 0 || i == _pGrid.xLength - 1 || j == _pGrid.yLength - 1 || k == _pGrid.zLength - 1))
                    {
                        int neighbourCount = GetNeighbourCount(i, j, k);
                        pGridNew.points[i,j,k].w = Eval(i, j, k, neighbourCount);
                    }
                }
            }
        }
        _pGrid = pGridNew;
    }

    int Eval(int x, int y, int z, int neighbourCount)
    {
        //Survival rules
        if (_pGrid.points[x, y, z].w == 0f)
        {
            for (int i = 0; i < survive.Length; i++)
            {
                if (neighbourCount == survive[i])
                {
                    //print($"Iter {iterTest} Survived- neighbour count : {neighbourCount}");
                    return 0;
                }
            }
        }
        //Birth rules
        else if (_pGrid.points[x, y, z].w == 1f)
        {
            for (int i = 0; i < birth.Length; i++)
            {
                if (neighbourCount == birth[i])
                {
                    //print($"Iter {iterTest} Born- neighbour count : {neighbourCount}");
                    return 0;
                }
            }
        }
        else
        {
            return 0;
        }
        //Default to dead
        return 1;
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
                            if(_pGrid.points[i,j,k].w == 0f) { neighbourCount++; }
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

    protected override void DelayedOnValidate()
    {
        return;
    }

}
