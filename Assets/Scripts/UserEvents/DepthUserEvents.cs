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

    protected override void OnMouseScroll(Vector2 scrollDelta)
    {
        base.OnMouseScroll(scrollDelta);
    }

    protected override void OnLeftMouseButtonClick(Vector3 localPoint)
    {
        base.OnLeftMouseButtonClick(localPoint);
    }

    protected override void OnRightMouseButtonDrag(Vector3 pointDelta)
    {
        base.OnRightMouseButtonDrag(pointDelta);
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        if (Input.GetMouseButton(0))
            ToolTipManager.Instance().DestoryDistanceInfo();
    }
}
