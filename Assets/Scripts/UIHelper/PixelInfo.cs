using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PixelInfo : MonoBehaviour
{
    private GameObject target;
    private Vector3 localPosition;
    private Func<string> updateToolTip;
    private void Awake()
    {

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
}
