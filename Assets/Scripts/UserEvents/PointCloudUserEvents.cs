using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudUserEvents : UserEventBase
{
    // Start is called before the first frame update
    GameObject center;
    protected override void Start()
    {
        center = this.gameObject.GetComponent<PointCloud>().PointCloudCenter;
        base.scaleSpeed = 200.1f;//�������0
        base.moveSpeed = 0.2f;
    }
    private void Awake()
    {

    }


    protected override void OnLeftMouseButtonDown(Vector3 localPoint)
    {

    }

    protected override void GetRightMouseButtonDown(Vector3 pointDelta)
    {
        if (Mathf.Abs(pointDelta.x) > Mathf.Abs(pointDelta.y))
            Camera.main.transform.RotateAround(center.transform.position, Vector3.up, pointDelta.x * moveSpeed);
        else
            Camera.main.transform.RotateAround(center.transform.position, Vector3.right, -pointDelta.y * moveSpeed);
    }
}