using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFC_Manager : MonoBehaviour
{
    public WFC_Tile[] tiles;
    public int gridWidth, gridHeight;
    WFC_Simple _simpleWFC;

    public void Run()
    {
        _simpleWFC.Run(gridWidth, gridHeight, tiles);
    }
}
