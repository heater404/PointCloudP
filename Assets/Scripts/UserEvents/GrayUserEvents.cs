using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrayUserEvents : UserEventBase
{
    protected override string ToolTipFormat(Vector2Int sn, float value)
    {
        return $"{sn}:\n{value}LSB";
    }

    protected override void OnLeftMouseButtonHoldDown(Vector3 start, Vector3 end)
    {

    }

    protected override void OnRightMouseButtonDrag(Vector3 pointDelta)
    {
        base.OnRightMouseButtonDrag(pointDelta);
    }

    protected override void OnLeftMouseButtonClick(Vector3 localPoint)
    {
        base.OnLeftMouseButtonClick(localPoint);
    }

    protected override void OnMouseScroll(Vector2 scrollDelta)
    {
        base.OnMouseScroll(scrollDelta);
    }

}
