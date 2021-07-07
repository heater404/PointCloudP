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

    protected override void OnRightMouseButtonHoldDown(Vector3 pointDelta)
    {
        base.OnRightMouseButtonHoldDown(pointDelta);
    }

    protected override void OnLeftMouseButtonDown(Vector3 localPoint)
    {
        base.OnLeftMouseButtonDown(localPoint);
    }

    protected override void MouseScroll(Vector2 scrollDelta)
    {
        base.MouseScroll(scrollDelta);
    }

}
