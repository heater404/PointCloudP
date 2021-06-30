using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraCtrl : MonoBehaviour
{
    public List<CameraParams> CamerasParams { get; set; } = new List<CameraParams>();

    float miniViewWidth;
    float topMenuHeight;

    // Use this for initialization
    void Start()
    {
        miniViewWidth = GameObject.Find("MiniView").GetComponent<RectTransform>().rect.width;
        topMenuHeight = GameObject.Find("TopMenu").GetComponent<RectTransform>().rect.height;
    }

    // Update is called once per frame
    void Update()
    {
        var x = miniViewWidth / Screen.width;
        var w = (Screen.width - miniViewWidth) / Screen.width;
        var h = (Screen.height - topMenuHeight) / Screen.height;
        this.gameObject.GetComponent<Camera>().rect = new Rect(x, 0, w, h);
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
