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

        //�ٽ���������ת��Ϊ��������
        //(1.0 / comm.PixelColumn)��ʾһ������ռ�Ĵ�С
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
    /// ��Ϊ���tooltip���������ͼ�ϵ� ��������Ӧ����������ͼ�ı�������  
    /// ���������ͼ�������š�ƽ�Ƶ�ʱ�򱾵������ǲ����
    /// Ȼ��ת��Ϊ�������꣬�����ͼ���š�ƽ��֮����Ҫ����ת��Ϊ�µ���������
    /// 
    /// �����ϢҲ��Ҫ��ÿһ֡�������»�ȡ
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
