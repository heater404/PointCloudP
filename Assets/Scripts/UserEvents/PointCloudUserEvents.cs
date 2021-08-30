using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudUserEvents : UserEventBase
{
    // Start is called before the first frame update
    void Start()
    {
        //center = this.gameObject.GetComponent<PointCloud>().PointCloudCenter;
        base.scaleSpeed = 200.1f;//±‹√‚≥ˆœ÷0
        base.moveSpeed = 0.2f;

    }

    protected override void OnRightMouseButtonDrag(Vector3 pointDelta)
    {
        Vector3 center = Camera.main.GetComponent<MainCameraCtrl>().PointCloudCenter;

        //if (Mathf.Abs(pointDelta.x) > Mathf.Abs(pointDelta.y))
        Camera.main.transform.RotateAround(center, Vector3.up, pointDelta.x * moveSpeed);
        //else
        Camera.main.transform.RotateAround(center, Vector3.right, -pointDelta.y * moveSpeed);
    }

    protected override void OnLeftMouseButtonDrag(Vector3 localPointStart, Vector3 localPointEnd)
    {

    }

    protected override void OnLeftMouseButtonClick(Vector3 localPoint, PixelInfoStatus status = PixelInfoStatus.Active)
    {

    }

    protected override void OnLeftControlAddLeftMouseButtonClick(Vector2Int pixelSN)
    {

    }

    protected override void OnLeftMouseButtonDrag(Vector2 delta)
    {
        Camera.main.transform.Translate(-delta * moveSpeed * 10);
    }
}
