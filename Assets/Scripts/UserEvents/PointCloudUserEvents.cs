using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudUserEvents : UserEventBase
{
    // Start is called before the first frame update
    GameObject center;
    void Start()
    {
        center = this.gameObject.GetComponent<PointCloud>().PointCloudCenter;
        base.scaleSpeed = 200.1f;//±ÜÃâ³öÏÖ0
        base.moveSpeed = 0.2f;
    }

    protected override void OnRightMouseButtonDrag(Vector3 pointDelta)
    {
        if (Mathf.Abs(pointDelta.x) > Mathf.Abs(pointDelta.y))
            Camera.main.transform.RotateAround(center.transform.position, Vector3.up, pointDelta.x * moveSpeed);
        else
            Camera.main.transform.RotateAround(center.transform.position, Vector3.right, -pointDelta.y * moveSpeed);
    }

    protected override void OnLeftMouseButtonDrag(Vector3 localPointStart, Vector3 localPointEnd)
    {
        
    }

    protected override void OnLeftMouseButtonClick(Vector3 localPoint, PixelInfoStatus status = PixelInfoStatus.Active)
    {
        
    }
}
