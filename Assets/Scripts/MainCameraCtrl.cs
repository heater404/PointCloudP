using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainCameraCtrl : MonoBehaviour
{
    public List<CameraParams> CamerasParams { get; set; } = new List<CameraParams>();
    public Button MainViewBtn;
    public Button TopViewBtn;
    public Button SideViewBtn;
    public Button[] ResetBtns;
    public Transform PointCloud;
    public RectTransform Left;
    public RectTransform Top;
    public RectTransform Right;
    public RectTransform Bottom;
    // Use this for initialization
    void Start()
    {
        foreach (var reset in ResetBtns)
        {
            reset.onClick.AddListener(ResetCurrentCameraParam);
        }

        MainViewBtn.onClick.AddListener(() =>
        {
            var camera = GetCurrentMiniViewCamera();
            Camera.main.transform.position = camera.transform.position;
            Camera.main.transform.eulerAngles = camera.transform.eulerAngles;
        });

        TopViewBtn.onClick.AddListener(() =>
        {
            var camera = GetCurrentMiniViewCamera();
            Camera.main.transform.position = camera.transform.position;
            Camera.main.transform.eulerAngles = camera.transform.eulerAngles;
            Camera.main.transform.RotateAround(PointCloud.parent.position, Vector3.right, 90);
        });

        SideViewBtn.onClick.AddListener(() =>
        {
            var camera = GetCurrentMiniViewCamera();
            Camera.main.transform.position = camera.transform.position;
            Camera.main.transform.eulerAngles = camera.transform.eulerAngles;
            Camera.main.transform.RotateAround(PointCloud.parent.position, Vector3.up, 90);

        });
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

    public void ResetCurrentCameraParam()
    {
        var camera = GetCurrentMiniViewCamera();
        Camera.main.transform.position = camera.transform.position;
        Camera.main.transform.eulerAngles = camera.transform.eulerAngles;
        Camera.main.orthographicSize = camera.orthographicSize;

    }

    Camera GetCurrentMiniViewCamera()
    {
        var count = Camera.allCamerasCount;
        for (int i = 0; i < count; i++)
        {
            if (i != 0)//非MainCamera
            {
                var camera = Camera.allCameras[i];
                if (camera.cullingMask == Camera.main.cullingMask)
                {
                    return camera;
                }
            }
        }

        return null;
    }

    public void SetCameraParam(string targetLayer)
    {
        SaveCurrentCameraParam();

        var camera = CamerasParams.Find(param => param.Layer == LayerMask.NameToLayer(targetLayer));

        Camera.main.transform.position = camera.Position;
        Camera.main.transform.eulerAngles = camera.EulerAngles;
        Camera.main.orthographicSize = camera.Size;
        Camera.main.cullingMask = 1 << camera.Layer;
    }

    private void SaveCurrentCameraParam()
    {
        var currentLayer = (int)Mathf.Log(Camera.main.cullingMask, 2);

        var camera = CamerasParams.Find(param => param.Layer == currentLayer);
        if (camera != null)
        {
            camera.Position = Camera.main.transform.position;
            camera.EulerAngles = Camera.main.transform.eulerAngles;
            camera.Size = Camera.main.orthographicSize;
        }
    }

    void Awake()
    {
        var count = Camera.allCamerasCount;
        for (int i = 0; i < count; i++)
        {
            if (i != 0)//非MainCamera
            {
                CameraParams camera = new CameraParams
                {
                    Position = Camera.allCameras[i].transform.position,
                    EulerAngles = Camera.allCameras[i].transform.eulerAngles,
                    Size = Camera.allCameras[i].orthographicSize,
                    Layer = (int)Mathf.Log(Camera.allCameras[i].cullingMask, 2),
                };
                CamerasParams.Add(camera);
            }
        }
    }
}

public class CameraParams
{
    public Vector3 Position { get; set; }
    public Vector3 EulerAngles { get; set; }
    public float Size { get; set; }
    public int Layer { get; set; }
}
