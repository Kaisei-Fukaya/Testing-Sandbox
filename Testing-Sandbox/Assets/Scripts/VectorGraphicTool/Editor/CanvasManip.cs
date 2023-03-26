using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class CanvasPan : MouseManipulator
{
    #region Init
    private Vector2 m_Start;
    protected VisualElement m_child;
    protected bool m_Active;

    public CanvasPan()
    {
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.MiddleMouse });
        m_Active = false;
    }
    #endregion

    #region Registrations
    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
    }
    #endregion

    #region OnMouseDown
    protected void OnMouseDown(MouseDownEvent e)
    {
        if (m_Active)
        {
            e.StopImmediatePropagation();
            return;
        }

        if (CanStartManipulation(e))
        {
            m_child = target.Children().FirstOrDefault();
            m_Start = target.ChangeCoordinatesTo(m_child, e.localMousePosition);
            m_Active = true;
            target.CaptureMouse();
            e.StopPropagation();
        }
    }
    #endregion

    #region OnMouseMove
    protected void OnMouseMove(MouseMoveEvent e)
    {
        if (!m_Active || !target.HasMouseCapture())
            return;

        Vector2 diff = target.ChangeCoordinatesTo(m_child, e.localMousePosition - m_Start);
        m_child.style.top = Mathf.Clamp(m_child.layout.y + diff.y, 0, target.layout.height - m_child.layout.height);
        m_child.style.left = Mathf.Clamp(m_child.layout.x + diff.x, 0, target.layout.width - m_child.layout.width);

        e.StopPropagation();
    }
    #endregion

    #region OnMouseUp
    protected void OnMouseUp(MouseUpEvent e)
    {
        if (!m_Active || !target.HasMouseCapture() || !CanStopManipulation(e))
            return;

        m_Active = false;
        target.ReleaseMouse();
        e.StopPropagation();
    }
    #endregion
}
