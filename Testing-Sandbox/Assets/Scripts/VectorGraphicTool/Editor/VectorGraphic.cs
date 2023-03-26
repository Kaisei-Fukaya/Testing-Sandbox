using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class VectorGraphic : VisualElement
{
    List<Vector2> points = new List<Vector2> { new Vector2(0f,0f), new Vector2(100f, 0f), new Vector2(100f, 100f), new Vector2(50f, 100f) };
    List<VectorPoint> handles;
    Color colour = new Color(255f,255f,255f);
    protected bool closed = false;


    public VectorGraphic()
    {
        generateVisualContent += OnGenerateVisualContent;
        handles = new List<VectorPoint>();
        EditorApplication.delayCall += InitHandles;
    }

    void InitHandles()
    {
        for (int i = 0; i < points.Count; i++)
        {
            var newHandle = new VectorPoint();
            newHandle.onMove += UpdatePoints;
            newHandle.AddToClassList("draggable-node");
            newHandle.AddManipulator(new PointDragger());
            contentContainer.Add(newHandle);
            handles.Add(newHandle);
        }
        EditorApplication.delayCall += SetHandlePos;
    }

    void SetHandlePos()
    {
        for (int i = 0; i < handles.Count; i++)
        {
            handles[i].style.left = points[i].x - handles[i].style.width.value.value / 2;
            handles[i].style.top = points[i].y - handles[i].style.height.value.value / 2;
        }
    }

    public void UpdatePoints()
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = new Vector2(handles[i].layout.x + handles[i].layout.width / 2, 
                handles[i].layout.y + handles[i].layout.height / 2);
        }
        MarkDirtyRepaint();
    }

    public void AddPoint()
    {

    }

    void CloseShape()
    {
        closed = true;
    }

    public void RemovePoint(int index)
    {
        handles.RemoveAt(index);
        points.RemoveAt(index);
        MarkDirtyRepaint();
    }

    void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        var mesh = mgc.Allocate(points.Count, 6);
        mesh.SetNextVertex(new Vertex() { position = new Vector3(points[0].x, points[0].y, Vertex.nearZ), tint = colour });
        mesh.SetNextVertex(new Vertex() { position = new Vector3(points[1].x, points[1].y, Vertex.nearZ), tint = colour });
        mesh.SetNextVertex(new Vertex() { position = new Vector3(points[2].x, points[2].y, Vertex.nearZ), tint = colour });
        mesh.SetNextVertex(new Vertex() { position = new Vector3(points[3].x, points[3].y, Vertex.nearZ), tint = colour });

        mesh.SetNextIndex(0);
        mesh.SetNextIndex(1);
        mesh.SetNextIndex(2);
        mesh.SetNextIndex(0);
        mesh.SetNextIndex(2);
        mesh.SetNextIndex(3);
    }
}
