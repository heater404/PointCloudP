using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraCtrl : MonoBehaviour
{
    float miniViewWidth;
    float topMenuHeight;
    // Use this for initialization
    void Start()
    {
        miniViewWidth = GameObject.Find("MiniView").GetComponent<RectTransform>().rect.width;
        topMenuHeight = GameObject.Find("TopMenu").GetComponent<RectTransform>().rect.height;
    }

    // Update is called once per frame
    void Update()
    {
        var x = miniViewWidth / Screen.width;
        var w = (Screen.width - miniViewWidth) / Screen.width;
        var h = (Screen.height - topMenuHeight) / Screen.height;
        this.gameObject.GetComponent<Camera>().rect = new Rect(x, 0, w, h);
    }
}
