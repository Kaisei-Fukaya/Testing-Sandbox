using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="WFC_Tile", menuName ="Wave Function Collapse/Tile")]
public class WFC_Tile : ScriptableObject
{
    [SerializeField] Sprite _image;
    public Sprite image { get { return _image; } }

    [SerializeField] int[] _edges;
    public int[] edges { get { return _edges; } }
}
