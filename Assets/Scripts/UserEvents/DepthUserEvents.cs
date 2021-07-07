using UnityEngine;
public class DepthUserEvents : UserEventBase
{
    protected override string ToolTipFormat(Vector2Int sn, float value)
    {
        return $"{sn}:\n{value}mm";
    }

    protected override void OnLeftMouseButtonHoldDown(Vector3 start, Vector3 end)
    {
        base.OnLeftMouseButtonHoldDown(start, end);
    }

    protected override void MouseScroll(Vector2 scrollDelta)
    {
        base.MouseScroll(scrollDelta);
    }

    protected override void OnLeftMouseButtonDown(Vector3 localPoint)
    {
        base.OnLeftMouseButtonDown(localPoint);
    }

    protected override void OnRightMouseButtonHoldDown(Vector3 pointDelta)
    {
        base.OnRightMouseButtonHoldDown(pointDelta);
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        if (Input.GetMouseButton(0))
            ToolTipManager.Instance().DestoryDistanceInfo();
    }
}
