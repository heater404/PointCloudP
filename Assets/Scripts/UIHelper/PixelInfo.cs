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
    private Func<string> updateToolTip;
    ToolTipManager manager;
    RawImage dot;
    private void Awake()
    {
        manager = ToolTipManager.Instance();
        dot = this.gameObject.transform.Find("Dot")?.gameObject.GetComponent<RawImage>();
        if (null == dot)
        {
            Debug.Log("Can't find  PixelInfo's Dot");
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target == null || localPosition == null || updateToolTip == null)
            return;

        this.gameObject.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = updateToolTip?.Invoke();
        this.gameObject.transform.position = LocalPositionToScreenPoint(localPosition);
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
    public void Show(Func<string> updateToolTip, Vector3 position, GameObject target)
    {
        this.target = target;
        this.updateToolTip = updateToolTip;
        this.localPosition = position;
    }

    private Vector3 LocalPositionToScreenPoint(Vector3 localPosition)
    {
        var worldPosition = target.transform.TransformPoint(localPosition);

        return Camera.main.WorldToScreenPoint(worldPosition);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && !Input.GetKey(KeyCode.LeftControl))
        {
            if (this.status == PixelInfoStatus.Active)
                this.Status = PixelInfoStatus.Fix;
            else
                this.Status = PixelInfoStatus.Active;
        }
        else if (eventData.button == PointerEventData.InputButton.Right && Input.GetKey(KeyCode.LeftControl))
        {
            manager.DestoryOnePixelInfo(this.gameObject);
        }
    }
}
