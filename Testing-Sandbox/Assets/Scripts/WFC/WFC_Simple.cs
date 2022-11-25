using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFC_Simple
{
    //No sample analysis. Adjacency rules are user determined.
    struct Cell
    {
        public WFC_Tile tile;
        public bool collapsed;
        public Cell(WFC_Tile tile)
        {
            this.tile = tile;
            this.collapsed = false;
        }
    }
    Cell[,] _gridSpace;
    WFC_Tile[] _tiles;

    public void Run(int _gridWidth, int _gridHeight, WFC_Tile[] tiles)
    {
        _gridSpace = new Cell[_gridWidth, _gridHeight];
        _tiles = tiles;
    }
}
