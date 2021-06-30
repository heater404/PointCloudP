using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainViewSwitcher : MonoBehaviour
{
    List<Toggle> toggles = new List<Toggle>();
    List<GameObject> uis = new List<GameObject>();
    void Awake()
    {
        var count = this.gameObject.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            var toggle = this.gameObject.transform.GetChild(i).GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    SwitchToView(toggle.gameObject.name);
                }
            });
            toggles.Add(toggle);
        }

        count = GameObject.Find("TopMenu").transform.childCount;
        for (int i = 0; i < count; i++)
        {
            uis.Add(GameObject.Find("TopMenu").transform.GetChild(i).gameObject);
        }

    }

    void SwitchToView(string name)
    {
        var layer = LayerMask.NameToLayer(name);
        Camera.main.GetComponent<Camera>().cullingMask = 1 << layer;//开启layer层
        foreach (var ui in uis)
        {
            if (ui.layer != LayerMask.NameToLayer("UI"))
            {
                if (ui.layer == layer)
                    ui.SetActive(true);
                else
                    ui.SetActive(false);
            }
        }

        foreach (var tool in ToolTipManager.Instance().ToolTips)
        {
            if (tool.layer == layer)
                tool.SetActive(true);
            else
                tool.SetActive(false);
        }
    }

    // Use this for initialization
    void Start()
    {
        SwitchToView(toggles[0].gameObject.name);
        toggles[0].isOn = true;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
