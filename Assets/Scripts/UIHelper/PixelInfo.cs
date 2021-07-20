using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PixelInfo : MonoBehaviour, IPointerClickHandler
{
    private PixelInfoStatus status;
    public PixelInfoStatus Status
    {
        get { return status; }
        set
        {
            if (PixelInfoStatus.Active == value)
                dot.color = Color.white;
            else if (PixelInfoStatus.Fix == value)
                dot.color = new Color(0.18f, 0.18f, 0.18f, 1f);
            status = value;
        }
    }
    private GameObject target;
    private Vector3 localPosition;
    private Func<Vector2Int, float> GetPixelValue;
    ToolTipManager manager;
    RawImage dot;
    Text text;
    Vector2Int sn;
    string unit;
    private void Awake()
    {
        manager = ToolTipManager.Instance();
        dot = this.gameObject.transform.Find("Dot")?.gameObject.GetComponent<RawImage>();
        if (null == dot)
        {
            Debug.Log("Can't find  PixelInfo's Dot");
        }
        text = this.gameObject.transform.GetChild(0).GetChild(0).GetComponent<Text>();
    }

    private Vector2Int LocalPointToPixelSN(Vector3 localPoint)
    {
        var comm = GameObject.Find("Manager").GetComponent<Communication>();

        var newPoint = localPoint + new Vector3(0.5f, -0.5f, 0);
        newPoint = new Vector3(newPoint.x, -newPoint.y, newPoint.z);

        //再将本地坐标转换为像素坐标
        //(1.0 / comm.PixelColumn)表示一个像素占的大小
        Vector2Int sn = new Vector2Int((int)(newPoint.x / (1.0 / comm.PixelWidth)), (int)(newPoint.y / (1.0 / comm.PixelHeight)));
        return sn;
    }

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(UpdatePixelInfo());
    }

    void UpdatePixelInfo()
    {
        if (target == null || localPosition == null || GetPixelValue == null)
            return;

        text.text = PixelInfoFormat(sn, GetPixelValue.Invoke(sn), unit);
        this.gameObject.transform.position = LocalPositionToScreenPoint(localPosition);
    }

    int frameCount = 0;
    // Update is called once per frame
    void Update()
    {
        if (++frameCount == 10)
        {
            UpdatePixelInfo();
            frameCount = 0;
        }
    }

    private string PixelInfoFormat(Vector2Int sn, float pixelValue, string unit)
    {
        return $"{sn}:\n{pixelValue}{unit}";
    }

    /// <summary>
    /// 因为这个tooltip是贴在深度图上的 所以坐标应该是相对深度图的本地坐标  
    /// 并且在深度图进行缩放、平移的时候本地坐标是不变的
    /// 然后转换为世界坐标，在深度图缩放、平移之后，需要重新转换为新的世界坐标
    /// 
    /// 深度信息也需要在每一帧进行重新获取
    /// </summary>
    /// <param name="message"></param>
    /// <param name="position"></param>
    public void Show(Func<Vector2Int, float> pixelValue, string unit, Vector3 position, GameObject target)
    {
        this.target = target;
        this.GetPixelValue = pixelValue;
        this.localPosition = position;
        this.sn = LocalPointToPixelSN(localPosition);
        this.unit = unit;
    }

    private Vector3 LocalPositionToScreenPoint(Vector3 localPosition)
    {
        var worldPosition = target.transform.TransformPoint(localPosition);

        return Camera.main.WorldToScreenPoint(worldPosition);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (this.status == PixelInfoStatus.Active)
                this.Status = PixelInfoStatus.Fix;
            else
                this.Status = PixelInfoStatus.Active;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            manager.DestoryOnePixelInfo(this.gameObject);
        }
    }
}
