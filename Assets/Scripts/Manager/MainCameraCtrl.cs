using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Linq;
using System.IO;
using System;

public class MainCameraCtrl : MonoBehaviour
{
    public List<CameraParams> CurrentCamerasParams { get; set; } = new List<CameraParams>();
    public Button MainViewBtn;
    public Button TopViewBtn;
    public Button SideViewBtn;
    public Button[] ResetBtns;
    public RectTransform Left;
    public RectTransform Top;
    public RectTransform Right;
    public RectTransform Bottom;
    // Use this for initialization

    public RangeSlider DepthRange;
    public BoxCollider PointCloudBox;
    public Vector3 PointCloudCenter { get; set; }

    void Start()
    {
        foreach (var reset in ResetBtns)
        {
            reset.onClick.AddListener(ResetToOriginCameraParam);
        }

        MainViewBtn.onClick.AddListener(() =>
        {
            var camera = GetCameraParams();
            Camera.main.transform.position = camera.Position;
            Camera.main.transform.eulerAngles = camera.EulerAngles;
        });

        TopViewBtn.onClick.AddListener(() =>
        {
            var camera = GetCameraParams();
            Camera.main.transform.position = camera.Position;
            Camera.main.transform.eulerAngles = camera.EulerAngles;
            Camera.main.transform.RotateAround(PointCloudCenter, Vector3.right, 90);
        });

        SideViewBtn.onClick.AddListener(() =>
        {
            var camera = GetCameraParams();
            Camera.main.transform.position = camera.Position;
            Camera.main.transform.eulerAngles = camera.EulerAngles;
            Camera.main.transform.RotateAround(PointCloudCenter, Vector3.up, 90);

        });

        var count = Camera.allCamerasCount;
        for (int i = 0; i < count; i++)
        {
            if (Camera.allCameras[i] != Camera.main)//非MainCamera
            {
                CameraParams camera = new CameraParams
                {
                    Position = Camera.allCameras[i].transform.position,
                    EulerAngles = Camera.allCameras[i].transform.eulerAngles,
                    Size = Camera.allCameras[i].orthographicSize,
                    Layer = (int)Mathf.Log(Camera.allCameras[i].cullingMask, 2),
                };
                CurrentCamerasParams.Add(camera);
            }
        }

        PointCloudCenter = PointCloudBox.center;

        DepthRange.OnValueChanged.AddListener((l, h) =>
        {
            SetPointCloudCenter(l, h);
        });
        SetPointCloudCenter(DepthRange.LowValue, DepthRange.HighValue);
    }

    private void SetPointCloudCenter(float l, float h)
    {
        var delta = (l + h) / 2 - PointCloudCenter.z;
        PointCloudCenter = new Vector3(PointCloudCenter.x, PointCloudCenter.y, (l + h) / 2);

        if (Camera.main.cullingMask == 1 << LayerMask.NameToLayer("PointCloud"))
        {
            var originPos = Camera.main.transform.position;
            Camera.main.transform.position = new Vector3(originPos.x, originPos.y, originPos.z + delta);
        }
        else
        {
            var camera = GetCameraParams("PointCloud");
            var originPos = camera.Position;

            camera.Position = new Vector3(originPos.x, originPos.y, originPos.z + delta);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var x = Left?.rect.width / Screen.width;
        var y = Bottom?.rect.height / Screen.height;
        var w = (Screen.width - Left?.rect.width - Right?.rect.width) / Screen.width;
        var h = (Screen.height - Top?.rect.height) / Screen.height;
        this.gameObject.GetComponent<Camera>().rect = new Rect
            (x.HasValue ? x.Value : 0, y.HasValue ? y.Value : 0, w.HasValue ? w.Value : 1, h.HasValue ? h.Value : 1);
    }

    private void ResetToOriginCameraParam()
    {
        SetMainCameraParam(GetMiniCameraParams());
    }


    private CameraParams GetMiniCameraParams()
    {
        var layer = (int)Mathf.Log(Camera.main.cullingMask, 2);

        return GetMiniCameraParams(LayerMask.LayerToName(layer));
    }

    private CameraParams GetMiniCameraParams(string targetLayer)
    {
        var camera = Camera.allCameras.Single(c => LayerMask.NameToLayer(targetLayer) == (int)Mathf.Log(c.cullingMask, 2)
         && c != Camera.main);

        return new CameraParams
        {
            Position = camera.transform.position,
            EulerAngles = camera.transform.eulerAngles,
            Size = camera.orthographicSize,
            Layer = LayerMask.NameToLayer(targetLayer),
        };
    }

    public void SetMainCameraParam(string targetLayer)
    {
        SaveMainCameraParam();

        var paras = GetCameraParams(targetLayer);

        SetMainCameraParam(paras);
    }

    private void SetMainCameraParam(CameraParams paras)
    {
        Camera.main.transform.position = paras.Position;
        Camera.main.transform.eulerAngles = paras.EulerAngles;
        Camera.main.orthographicSize = paras.Size;
        Camera.main.cullingMask = 1 << paras.Layer;
    }

    private void SaveMainCameraParam()
    {
        var paras = GetCameraParams();
        if (paras != null)
        {
            paras.Position = Camera.main.transform.position;
            paras.EulerAngles = Camera.main.transform.eulerAngles;
            paras.Size = Camera.main.orthographicSize;
        }
    }

    private CameraParams GetCameraParams(string layerName)
    {
        var layer = LayerMask.NameToLayer(layerName);
        var paras = CurrentCamerasParams.Find(param => param.Layer == layer);
        return paras;
    }

    private CameraParams GetCameraParams()
    {
        var layer = (int)Mathf.Log(Camera.main.cullingMask, 2);
        var paras = CurrentCamerasParams.Find(param => param.Layer == layer);
        return paras;
    }
}

public class CameraParams
{
    public Vector3 Position { get; set; }
    public Vector3 EulerAngles { get; set; }
    public float Size { get; set; }
    public int Layer { get; set; }
}
