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
    /// 因为这个tooltip是贴在深度图上的 所以坐标应该是相对深度图的本地坐标  
    /// 并且在深度图进行缩放、平移的时候本地坐标是不变的
    /// 然后转换为世界坐标，在深度图缩放、平移之后，需要重新转换为新的世界坐标
    /// 
    /// 深度信息也需要在每一帧进行重新获取
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
