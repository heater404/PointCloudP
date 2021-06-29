using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ToolTip
{
    public GameObject Object { get; set; }
    public Func<string> UpdateToolTip { get; set; }
    public Vector3 LocalPosition { get; set; }

}

public class ToolTipManager : MonoBehaviour
{
    ToolTip[] tips;
    int lastShowIndex = -1;
    private void Awake()
    {
        tips = new ToolTip[2]
            {
                new ToolTip(),
                new ToolTip(),
            };
        for (int i = 0; i < tips.Length; i++)
        {
            tips[i].Object = (GameObject)Instantiate(Resources.Load("Prefabs/ToolTipCanvas", typeof(GameObject)));
            tips[i].Object.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (var tip in tips)
        {
            if (tip.Object.activeInHierarchy)
            {
                tip.Object.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = tip.UpdateToolTip?.Invoke();
                tip.Object.transform.GetChild(0).transform.position = LocalPositionToScreenPoint(tip.LocalPosition);
            }
        }
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
    public void Show(Func<string> updateToolTip, Vector3 position)
    {
        Debug.Log(position);
        tips[++lastShowIndex].UpdateToolTip = updateToolTip;
        tips[lastShowIndex].LocalPosition = position;
        tips[lastShowIndex].Object.SetActive(true);
        if (lastShowIndex == tips.Length - 1)
            lastShowIndex = -1;
    }

    private Vector3 LocalPositionToScreenPoint(Vector3 localPosition)
    {
        var worldPosition = this.gameObject.transform.TransformPoint(localPosition);

        //var scale = this.gameObject.transform.localScale;
        //worldPosition = new Vector3(worldPosition.x * scale.x, worldPosition.y * scale.y, worldPosition.z * scale.z);

        return Camera.main.WorldToScreenPoint(worldPosition);
    }
    public void Destroy()
    {
        for (int i = 0; i < tips.Length; i++)
        {
            Destroy(tips[i].Object);
        }
    }
}
