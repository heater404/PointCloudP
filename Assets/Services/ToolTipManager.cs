using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTipManager : MonoBehaviour
{
    void Awake()
    {
        
    }
    public static ToolTipManager Instance()
    {
        return GameObject.Find("ToolTipManager").GetComponent<ToolTipManager>();
    }

    const int MaxToolTipsNumPerLayer = 2;
    public List<GameObject> ToolTips { get; private set; } = new List<GameObject>();
    public void Show(Func<string> updateToolTip, Vector3 position, GameObject target)
    {
        var tooltip = (GameObject)Instantiate(Resources.Load("Prefabs/ToolTip", typeof(GameObject)));

        tooltip.GetComponent<ToolTip>().Show(updateToolTip, position, target);

        tooltip.layer = (int)Mathf.Log(Camera.main.cullingMask, 2);
        ToolTips.Add(tooltip);

        //超过个数就删除
        var res = ToolTips.FindAll(tool => tool.layer == tooltip.layer);
        if (res != null)
        {
            if (res.Count > MaxToolTipsNumPerLayer)
            {
                GameObject.DestroyImmediate(res[0]);
                ToolTips.Remove(res[0]);
            }
        }
    }
}
