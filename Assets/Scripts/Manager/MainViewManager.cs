using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainViewManager : MonoBehaviour
{
    public Transform Parent;
    List<Toggle> toggles = new List<Toggle>();
    void Awake()
    {
        var count = Parent.childCount;
        for (int i = 0; i < count; i++)
        {
            var toggle = Parent.GetChild(i).GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    SwitchToView(toggle.gameObject.name);
                }
            });
            toggles.Add(toggle);
        }
    }

    void SwitchToView(string name)
    {
        var Layer = LayerMask.NameToLayer(name);
        Camera.main.GetComponent<MainCameraCtrl>().SetCameraParam(name);//开启layer层

        GameObject.Find("Manager").GetComponent<UIManager>().SetActive(Layer);
    }

    // Use this for initialization
    void Start()
    {
        SwitchToView(toggles[0].gameObject.tag);
        toggles[0].isOn = true;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
