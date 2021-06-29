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
        //��껬�ֽ�������
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

        //��ס����Ҽ�����ƽ��
        if (Input.GetMouseButton(1))
        {
            Vector3 currentPoint = Input.mousePosition;

            Camera.main.transform.Translate(-(currentPoint.x - lastPoint.x) * moveSpeed, 0, 0);
            Camera.main.transform.Translate(0, -(currentPoint.y - lastPoint.y) * moveSpeed, 0);
            lastPoint = Input.mousePosition;
        }
    }


    //��������ײ��
    void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayhit;
        if (Physics.Raycast(ray, out rayhit))
        {
            //����������ת��Ϊ��������,�������������ԭ����Quad�����ĵ� 
            //�������������������Ͻ�Ϊ�������ĵģ����Ի���Ҫ�������������һ��ƫ��
            Vector3 localPoint = this.gameObject.transform.InverseTransformPoint(rayhit.point);

            var newPoint = localPoint + new Vector3(0.5f, -0.5f, 0);
            newPoint = new Vector3(newPoint.x, -newPoint.y, newPoint.z);

            //�ٽ���������ת��Ϊ��������
            //(1.0 / comm.PixelColumn)��ʾһ������ռ�Ĵ�С
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
