using UnityEngine;
public class DepthUserEvents : UserEventBase
{
    protected override void OnLeftMouseButtonDrag(Vector3 localPointStart, Vector3 localPointEnd)
    {
        var pixelSNStart = LocalPointToPixelSN(localPointStart);
        var pixelSNEnd = LocalPointToPixelSN(localPointEnd);

        ToolTipManager.Instance().ShowDistanceInfo(helper.GetPointData, helper.GetBufferData, localPointStart, localPointEnd, this.gameObject);
    }
}
