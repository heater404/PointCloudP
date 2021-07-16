using UnityEngine;
public class DepthUserEvents : UserEventBase
{
    protected override void Awake()
    {
        base.Awake();
        this.gameObject.transform.localScale = new Vector3(comm.PixelWidth / (comm.PixelHeight * 1.0f), 1, 1);
    }
    protected override void OnLeftMouseButtonDrag(Vector3 localPointStart, Vector3 localPointEnd)
    {
        var pixelSNStart = LocalPointToPixelSN(localPointStart);
        var pixelSNEnd = LocalPointToPixelSN(localPointEnd);

        ToolTipManager.Instance().ShowDistanceInfo(helper.GetPointData, helper.GetBufferData, localPointStart, localPointEnd, this.gameObject);
    }
}
