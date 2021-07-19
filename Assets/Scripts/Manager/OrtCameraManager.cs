using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrtCameraManager : MonoBehaviour
{
    public GameObject Obj;//要包围的物体
    Camera SetCamera;//正交相机
    public float ScreenScaleFactor;//占屏比例系数
    const float SizeMargin= 0.05f;
    private void Awake()
    {
        SetCamera = this.GetComponent<Camera>();
    }

    private void Start()
    {
        var bound = GetBoundPointsByObj(Obj);
        var center = bound.center;
        var extents = bound.extents;
        SetCamera.transform.position = new Vector3(center.x, center.y, center.z - 10);
        SetOrthCameraSize((center.x - extents.x), (center.x + extents.x), (center.y - extents.y), (center.y + extents.y));
    }
    /// <summary>
    /// 获取物体包围盒
    /// </summary>
    /// <param name="obj">父物体</param>
    /// <returns>物体包围盒</returns>
    private Bounds GetBoundPointsByObj(GameObject obj)
    {
        var bounds = new Bounds();
        if (obj != null)
        {
            var renders = obj.GetComponentsInChildren<Renderer>();
            if (renders != null)
            {
                var boundscenter = Vector3.zero;
                foreach (var item in renders)
                {
                    boundscenter += item.bounds.center;
                }
                if (obj.transform.childCount > 0)
                    boundscenter /= obj.transform.childCount;
                bounds = new Bounds(boundscenter, Vector3.zero);
                foreach (var item in renders)
                {
                    bounds.Encapsulate(item.bounds);
                }
            }
        }
        return bounds;
    }
    /// <summary>
    /// 设置正交相机的Size
    /// </summary>
    /// <param name="xmin">包围盒x方向最小值</param>
    /// <param name="xmax">包围盒x方向最大值</param>
    /// <param name="ymin">包围盒y方向最小值</param>
    /// <param name="ymax">包围盒y方向最大值</param>
    private void SetOrthCameraSize(float xmin, float xmax, float ymin, float ymax)
    {
        float xDis = xmax - xmin;
        float yDis = ymax - ymin;
        float sizeX = xDis / ScreenScaleFactor / 2 / SetCamera.aspect;
        float sizeY = yDis / ScreenScaleFactor / 2;
        if (sizeX >= sizeY)
            SetCamera.orthographicSize = sizeX + SizeMargin;
        else
            SetCamera.orthographicSize = sizeY + SizeMargin;
    }
}
