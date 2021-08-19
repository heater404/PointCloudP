using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ColorBarMenu : MonoBehaviour
{
    ///如果要让Hierarchy里面的Gameobject通过鼠标右键单击
    ///弹出对话框中出现该选项,则需要将该选项加入到"GameObject"目录下
    [MenuItem("GameObject/UI/ColorBar")]
    public static void AddColorBarInGameObject()
    {

        GameObject parent = null;
        if (null != Selection.activeTransform)
        {
            parent = Selection.activeTransform.gameObject;
        }
        else
        {
            parent = null;
        }

        if ((null == parent) || (null == parent.GetComponentInParent<Canvas>()))
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (null == canvas)
            {
                Debug.LogError("AddColorBar : can not find a canvas in scene!");
                return;
            }
            else
            {
                parent = FindObjectOfType<Canvas>().gameObject;
            }
        }

        GameObject prefab = Resources.Load("Prefabs/ColorBar") as GameObject;
        if (null == prefab)
        {
            Debug.LogError("AddColorBar : Load ColorBar Error!");
            return;
        }

        GameObject colorBar;
        if (null != parent)
            colorBar = Instantiate(prefab, parent.transform);
        else
            colorBar = Instantiate(prefab);

        if (null == colorBar)
        {
            Debug.LogError("AddColorBar : Instantiate ColorBar Error!");
            return;
        }

        Undo.RegisterCreatedObjectUndo(colorBar, "Created ColorBar");
        colorBar.name = "ColorBar";
    }
}
