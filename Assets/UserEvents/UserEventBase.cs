using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UserEventBase : MonoBehaviour
{
    protected Communication comm;
    float scaleSpeed = 0.2f;
    float moveSpeed = 0.003f;
    protected TwoDShaderHelperBase helper;
    // Start is called before the first frame update
    void Awake()
    {
        comm = GameObject.Find("Communication").GetComponent<Communication>();
    }
    protected virtual void Start()
    {
        this.gameObject.transform.localScale = new Vector3(comm.PixelWidth / (comm.PixelHeight * 1.0f), 1, 1) * 1.5f;

        helper = this.gameObject.GetComponent<TwoDShaderHelperBase>();
    }

    // Update is called once per frame
    Vector3 lastPoint;
    // Update is called once per frame
    void Update()
    {

    }

    protected virtual void OnDestroy()
    {

    }

    private void OnMouseOver()
    {
        //鼠标滑轮进行缩放
        if (Input.mouseScrollDelta != Vector2.zero)
        {
            Vector2 scroll = Input.mouseScrollDelta;
            if (scroll.y > 0)
                Camera.main.orthographicSize -= scaleSpeed;
            else
                Camera.main.orthographicSize += scaleSpeed;
        }

        if (Input.GetMouseButtonDown(1))
        {
            lastPoint = Input.mousePosition;
        }

        //按住鼠标右键进行平移
        if (Input.GetMouseButton(1))
        {
            Vector3 currentPoint = Input.mousePosition;

            Camera.main.transform.Translate(-(currentPoint.x - lastPoint.x) * moveSpeed, 0, 0);
            Camera.main.transform.Translate(0, -(currentPoint.y - lastPoint.y) * moveSpeed, 0);
            lastPoint = Input.mousePosition;
        }
    }


    //左键点击碰撞体
    void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayhit;
        if (Physics.Raycast(ray, out rayhit))
        {
            //将世界坐标转换为本地坐标,本地坐标的坐标原点是Quad的中心点 
            //但是像素坐标是以左上角为坐标中心的，所以还需要将本地坐标添加一个偏移
            Vector3 localPoint = this.gameObject.transform.InverseTransformPoint(rayhit.point);

            var newPoint = localPoint + new Vector3(0.5f, -0.5f, 0);
            newPoint = new Vector3(newPoint.x, -newPoint.y, newPoint.z);

            //再将本地坐标转换为像素坐标
            //(1.0 / comm.PixelColumn)表示一个像素占的大小
            Vector2Int sn = new Vector2Int((int)(newPoint.x / (1.0 / comm.PixelWidth)), (int)(newPoint.y / (1.0 / comm.PixelHeight)));
            
            this.gameObject.GetComponent<ToolTipManager>().Show(() =>
            {
                var data = helper.GetBufferData(sn);

                return ToolTipFormat(sn, data);
            }, localPoint);
        }
    }

    protected virtual string ToolTipFormat(Vector2Int sn, float value)
    {
        return "";
    }
}
