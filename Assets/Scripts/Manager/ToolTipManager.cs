using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipManager : MonoBehaviour
{
    public Button[] ClearBtns;
    public Transform ToolTipsParent;
    void Awake()
    {
        foreach (var clearBtn in ClearBtns)
        {
            clearBtn?.onClick.AddListener(() =>
            {
                DestoryCurrentPixelInfo(); 
                DestoryDistanceInfo();
            });
        }
    }
    public static ToolTipManager Instance()
    {
        return GameObject.Find("Manager").GetComponent<ToolTipManager>();
    }

    const int MaxToolTipsNumPerLayer = 2;
    List<GameObject> pixelInfos = new List<GameObject>();
    List<GameObject> distancesInfos = new List<GameObject>();
    public void ShowPixelInfo(Func<string> updateToolTip, Vector3 position, GameObject target)
    {
        var tooltip = (GameObject)Instantiate(Resources.Load("Prefabs/PixelInfo", typeof(GameObject)), Vector3.zero, Quaternion.Euler(0, 0, 0), ToolTipsParent);
        tooltip.transform.SetSiblingIndex(0);//设置子物体的index
        tooltip.GetComponent<ToolTip>().Show(updateToolTip, position, target);
        tooltip.layer = (int)Mathf.Log(Camera.main.cullingMask, 2);
        pixelInfos.Add(tooltip);

        //超过个数就删除
        var res = pixelInfos.FindAll(tool => tool.layer == tooltip.layer);
        if (res != null)
        {
            if (res.Count > MaxToolTipsNumPerLayer)
            {
                pixelInfos.Remove(res[0]);
                GameObject.DestroyImmediate(res[0]);
            }
        }
    }

    public void ShowDistanceInfo(Func<float> getDistance, Vector3 start, Vector3 end, GameObject target)
    {
        if (distancesInfos.Count > 0)
        {
            GameObject.DestroyImmediate(distancesInfos[0]);
            distancesInfos.Clear();
        }
        var distance = (GameObject)Instantiate(Resources.Load("Prefabs/DistanceMeasurement", typeof(GameObject)), Vector3.zero, Quaternion.Euler(0, 0, 0), ToolTipsParent);
        distance.transform.SetSiblingIndex(0);//设置子物体的index
        distance.GetComponent<DistanceMeasurement>().Show(start, end, getDistance, target);
        distance.layer = (int)Mathf.Log(Camera.main.cullingMask, 2);
        distancesInfos.Add(distance);
    }

    public void DestoryDistanceInfo()
    {
        if (distancesInfos.Count > 0)
        {
            GameObject.DestroyImmediate(distancesInfos[0]);
            distancesInfos.Clear();
        }
    }

    private void DestoryCurrentPixelInfo()
    {
        var res = pixelInfos.FindAll(tool => tool.layer == (int)Mathf.Log(Camera.main.cullingMask, 2));
        if (res != null)
        {
            pixelInfos.RemoveAll(tool => tool.layer == (int)Mathf.Log(Camera.main.cullingMask, 2));
            for (int i = 0; i < res.Count; i++)
            {
                GameObject.DestroyImmediate(res[i]);
            }
        }
    }
}
