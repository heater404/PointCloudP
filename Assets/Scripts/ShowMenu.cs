using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMenu : MonoBehaviour
{
    public Button ShowBtn;
    // Use this for initialization
    public GameObject Menu;
    void Start()
    {
        if (ShowBtn == null || Menu == null)
        {
            Debug.Log("ShowBtn == null || Menu == null");
            return;
        }
        Menu.SetActive(false);
        ShowBtn.onClick.AddListener(() =>
        {
            Menu.SetActive(!Menu.activeInHierarchy);
        });

        //点击菜单中一个按钮后自动关闭Menu
        var count = Menu.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            var btn = Menu.transform.GetChild(i).gameObject.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    Menu.SetActive(!Menu.activeInHierarchy);
                });
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
