using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    static Dictionary<int, List<int>> TargetLayerToUILayer = new Dictionary<int, List<int>>
    {
        { 8, new List<int> { 8, 11 }},
        { 9, new List<int> { 9 }},
        { 10, new List<int> { 10, 11 }},
    };

    // Use this for initialization
    public GameObject[] UIs;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetActive(int layer)
    {
        foreach (var ui in UIs)
        {
            SetActive(ui, layer);
        }
    }

    /// <summary>
    /// UI层的激活 如果layer为UI则不需要管理
    /// 不为UI的layer 根据当前相机的Layer进行管理
    /// </summary>
    /// <param name="game"></param>
    /// <param name="targetLayer"></param>
    void SetActive(GameObject game, int targetLayer)
    {
        List<int> uiLayers = new List<int>();
        if (TargetLayerToUILayer.TryGetValue(targetLayer, out uiLayers))
        {
            if (game.layer == LayerMask.NameToLayer("UI"))
            {
                var count = game.transform.childCount;
                if (count == 0)
                    return;
                for (int i = 0; i < count; i++)
                {
                    var child = game.transform.GetChild(i).gameObject;

                    SetActive(child, targetLayer);
                }
            }
            else
                game.SetActive(uiLayers.Contains(game.layer));
        }
    }
}
