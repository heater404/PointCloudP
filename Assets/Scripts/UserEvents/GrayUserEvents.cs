using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrayUserEvents : UserEventBase
{
    protected override void Awake()
    {
        base.Awake();
        this.gameObject.transform.localScale = new Vector3(comm.PixelWidth / (comm.PixelHeight * 1.0f), 1, 1);
    }
    protected override void OnLeftMouseButtonDrag(Vector3 localPointStart, Vector3 localPointEnd)
    {
        
    }
}
