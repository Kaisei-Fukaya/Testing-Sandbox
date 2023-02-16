using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SSL;
using System;

[ExecuteInEditMode()]
public class BezierHelper : MonoBehaviour
{
    GraphReader _currentGraphReader;
    List<SElement.BezierPoint> _bezierPoints = new List<SElement.BezierPoint>();

    private void OnEnable()
    {
        if(TryGetComponent(out GraphReader gR))
        {
            _currentGraphReader = gR;
        }
    }

    private void OnDrawGizmos()
    {
        if (_currentGraphReader == null)
            return;

        _bezierPoints = _currentGraphReader.GetBezierPoints();

        Vector3 objectPosition = transform.position;

        foreach (var point in _bezierPoints)
        {
            Vector3 actualPos = objectPosition + point.position;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(actualPos, actualPos + point.normal);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(actualPos, actualPos + point.tangent);
        }
    }
}
