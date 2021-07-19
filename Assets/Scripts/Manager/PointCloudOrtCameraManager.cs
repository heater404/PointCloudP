using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudOrtCameraManager : OrtCameraManager {

    protected override Bounds GetBoundPointsByObj(GameObject obj)
    {
        //var box = base.Obj.GetComponent<BoxCollider>();

        //return new Bounds(box.center, box.size);

        return base.GetBoundPointsByObj(obj);
    }
}
