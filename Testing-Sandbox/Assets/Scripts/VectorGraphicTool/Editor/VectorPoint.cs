using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VectorPoint : VisualElement
{
    public delegate void OnMoveAction();
    public event OnMoveAction onMove;
    public VectorPoint()
    {
    }

    public void Move()
    {
        onMove?.Invoke();
    }
}