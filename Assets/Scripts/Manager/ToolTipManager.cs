using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//用于表示PixelInfo的状态
public enum PixelInfoStatus
{
    Fix,//固定不会被刷新
    Active,//如果超出预设的数量则会被刷新
}

public class ToolTipManager : MonoBehaviour
{
    public Button[] ClearBtns;
    public Transform ToolTipsParent;
    GameObject pixelInfoPrefab;
    GameObject distanceInfoPrefab;

    void Awake()
    {
        foreach (var clearBtn in ClearBtns)
        {
            clearBtn?.onClick.AddListener(() =>
            {
                DestoryAllCurrentPixelInfo();
                DestoryDistanceInfo();
            });
        }

        pixelInfoPrefab = (GameObject)Resources.Load("Prefabs/PixelInfo", typeof(GameObject));
        distanceInfoPrefab = (GameObject)Resources.Load("Prefabs/DistanceInfo", typeof(GameObject));
    }
    public static ToolTipManager Instance()
    {
        return GameObject.Find("Manager").GetComponent<ToolTipManager>();
    }

    const int MaxToolTipsNumPerLayer = 5;
    List<GameObject> pixelInfos = new List<GameObject>();
    List<GameObject> distancesInfos = new List<GameObject>();
    public void ShowPixelInfo(Func<string> updateToolTip, Vector3 position, GameObject target)
    {
        GameObject tooltip = CreatOnePixelInfo(updateToolTip, position, target);

        //超过个数就删除
        var res = pixelInfos.FindAll(tool => tool.layer == tooltip.layer);
        if (res != null)
        {
            if (res.Count > MaxToolTipsNumPerLayer)
                DestoryFirstActivePixelInfo();
        }
    }

    private GameObject CreatOnePixelInfo(Func<string> updateToolTip, Vector3 position, GameObject target)
    {
        var pixelInfo = (GameObject)Instantiate(pixelInfoPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0), ToolTipsParent);
        pixelInfo.transform.SetSiblingIndex(0);//设置子物体的index
        var p = pixelInfo.GetComponent<PixelInfo>();
        p.Status = PixelInfoStatus.Active;
        p.Show(updateToolTip, position, target);
        pixelInfo.layer = (int)Mathf.Log(Camera.main.cullingMask, 2);
        pixelInfos.Add(pixelInfo);
        return pixelInfo;
    }

    public void ShowDistanceInfo(Func<float> getDistance, Vector3 start, Vector3 end, GameObject target)
    {
        if (distancesInfos.Count > 0)
        {
            GameObject.DestroyImmediate(distancesInfos[0]);
            distancesInfos.Clear();
        }
        var distance = (GameObject)Instantiate(distanceInfoPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0), ToolTipsParent);
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

    public void DestoryOnePixelInfo(GameObject pixelInfo)
    {
        var res = pixelInfos.FindAll(tool => tool.layer == (int)Mathf.Log(Camera.main.cullingMask, 2));
        if (res != null)
        {
            if (res.Contains(pixelInfo))
            {
                pixelInfos.Remove(pixelInfo);
                DestroyImmediate(pixelInfo);
            }
        }
    }

    private void DestoryFirstActivePixelInfo()
    {
        var res = pixelInfos.FindAll(tool => tool.layer == (int)Mathf.Log(Camera.main.cullingMask, 2));
        if (res != null)
        {
            var pixelInfo= res.Find(p => p.GetComponent<PixelInfo>().Status == PixelInfoStatus.Active);
            if (null != pixelInfo)
            {
                pixelInfos.Remove(pixelInfo);
                DestroyImmediate(pixelInfo);
            }
        }
    }

    private void DestoryAllCurrentPixelInfo()
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
