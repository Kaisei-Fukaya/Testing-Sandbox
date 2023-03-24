using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VectorGraphic : VisualElement
{
    Vector2[] points = new Vector2[] { new Vector2(0f,0f), new Vector2(100f, 0f), new Vector2(100f, 100f), new Vector2(50f, 100f) };
    VectorPoint[] handles;
    Color colour = new Color(255f,255f,255f);


    public VectorGraphic()
    {
        generateVisualContent += OnGenerateVisualContent;
        handles = new VectorPoint[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            handles[i] = new VectorPoint();
            handles[i].onMove += UpdatePoints;
            handles[i].AddToClassList("draggable-node");
            handles[i].layout.Set(points[i].x, points[i].y, handles[i].layout.width, handles[i].layout.height);
            handles[i].AddManipulator(new PointDragger());
            contentContainer.Add(handles[i]);
        }
    }

    public void UpdatePoints()
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i].x = handles[i].layout.x + handles[i].layout.width / 2;
            points[i].y = handles[i].layout.y + handles[i].layout.height / 2;
        }
        MarkDirtyRepaint();
    }

    void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        var mesh = mgc.Allocate(4, 6);
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
