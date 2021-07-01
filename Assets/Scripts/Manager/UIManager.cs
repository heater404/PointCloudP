using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Use this for initialization
    public GameObject[] UIs;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActive(int targetLayer)
    {
        foreach (var ui in UIs)
        {
            SetActive(ui, targetLayer);
        }
    }

    /// <summary>
    /// UI层的激活 如果layer为UI则不需要管理
    /// 不为UI的layer 根据当前相机的Layer进行管理
    /// </summary>
    /// <param name="game"></param>
    /// <param name="layer"></param>
    void SetActive(GameObject game, int layer)
    {
        //如果父物体Layer为ui,则需要找他的子物体
        if (game.layer == LayerMask.NameToLayer("UI"))
        {
            var count = game.transform.childCount;
            if (count == 0)
                return;
            for (int i = 0; i < count; i++)
            {
                var child = game.transform.GetChild(i);
                SetActive(child.gameObject, layer);
            }
        }
        else
            game.SetActive(game.layer == layer);
    }
}
