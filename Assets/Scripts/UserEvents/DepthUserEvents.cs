using UnityEngine;
using System;
public class DepthUserEvents : UserEventBase
{
    public event EventHandler<Vector2Int> CaptureOnePixelContinue;
    protected override void Awake()
    {
        base.Awake();
        this.gameObject.transform.localScale = new Vector3(comm.PixelWidth / (comm.PixelHeight * 1.0f), 1, 1);
    }

    protected override void OnLeftControlAddLeftMouseButtonClick(Vector2Int pixelSN)
    {
        this.CaptureOnePixelContinue?.Invoke(null, pixelSN);
    }

    protected override void OnLeftMouseButtonDrag(Vector3 localPointStart, Vector3 localPointEnd)
    {
        ToolTipManager.Instance().ShowDistanceInfo(Helper.GetPointData, Helper.GetBufferData, localPointStart, localPointEnd, this.gameObject);
    }
}
