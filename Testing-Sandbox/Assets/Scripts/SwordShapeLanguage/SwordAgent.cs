using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using SSL;
using System.Linq;

public class SwordAgent : Agent
{
    SwordGraph sg;
    public override void Initialize()
    {
        sg = new SwordGraph();
        base.Initialize();
    }
    //public override void OnActionReceived(ActionBuffers )
    //{
    //    sg.Load(1, 0f, );
    //}
    public override void CollectObservations(VectorSensor sensor)
    {
        //Execute queued generation
        Mesh mesh = sg.Generate();
        var verts = mesh.vertices;
        var tris = mesh.triangles;

        //Collect mesh data as observations
        sensor.AddObservation(verts.Cast<float>());
        sensor.AddObservation(tris.Cast<float>());

        //Request new input
        RequestDecision();
    }
}
