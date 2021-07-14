using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UserEventBase : MonoBehaviour, IPointerClickHandler, IDragHandler, IDropHandler, IBeginDragHandler,IScrollHandler
{
    protected Communication comm;
    protected float scaleSpeed = 0.19f;//避免出现零
    protected float moveSpeed = 0.003f;
    protected ShaderHelperBase helper;
    // Start is called before the first frame update
    void Awake()
    {
        comm = GameObject.Find("Manager").GetComponent<Communication>();
    }
    protected virtual void Start()
    {
        this.gameObject.transform.localScale = new Vector3(comm.PixelWidth / (comm.PixelHeight * 1.0f), 1, 1) * 1.5f;

        helper = this.gameObject.GetComponent<ShaderHelperBase>();
    }

    Vector3? leftMouseButtonDownLastPoint = null;
    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            leftMouseButtonDownLastPoint = Input.mousePosition;
        }


        //按住鼠标左键画线
        if (Input.GetMouseButton(0))
        {
            if (leftMouseButtonDownLastPoint == null)
                return;

            Vector3 currentPoint = Input.mousePosition;

            var delta = currentPoint - leftMouseButtonDownLastPoint.Value;

            if (Mathf.Abs(delta.x) < 2 && Mathf.Abs(delta.y) < 2)
                return;

            OnLeftMouseButtonHoldDown(leftMouseButtonDownLastPoint.Value, currentPoint);
        }

        //鼠标左键抬起
        if (Input.GetMouseButtonUp(0))
        {
            if (leftMouseButtonDownLastPoint == null)
                return;
            var delta = Input.mousePosition - leftMouseButtonDownLastPoint.Value;

            leftMouseButtonDownLastPoint = null;

            if (Mathf.Abs(delta.x) < 2 && Mathf.Abs(delta.y) < 2)
                return;

            //OnMouseDown();
        }
    }

    protected virtual void OnMouseScroll(Vector2 scrollDelta)
    {
        if (scrollDelta.y > 0)
        {
            Camera.main.orthographicSize -= scaleSpeed;
            if (Camera.main.orthographicSize < 0)
                Camera.main.orthographicSize += scaleSpeed;
        }
        else
            Camera.main.orthographicSize += scaleSpeed;
    }

    protected virtual void OnRightMouseButtonDrag(Vector3 pointDelta)
    {
        Camera.main.transform.Translate(-(pointDelta.x) * moveSpeed, 0, 0);
        Camera.main.transform.Translate(0, -(pointDelta.y) * moveSpeed, 0);
    }

    protected virtual void OnLeftMouseButtonHoldDown(Vector3 start, Vector3 end)
    {
        Ray rayStart = Camera.main.ScreenPointToRay(start);
        Ray rayEnd = Camera.main.ScreenPointToRay(end);
        RaycastHit rayhit;
        if (Physics.Raycast(rayStart, out rayhit))
        {
            //将世界坐标转换为本地坐标,本地坐标的坐标原点是Quad的中心点 
            //但是像素坐标是以左上角为坐标中心的，所以还需要将本地坐标添加一个偏移
            Vector3 localPointStart = this.gameObject.transform.InverseTransformPoint(rayhit.point);
            if (Physics.Raycast(rayEnd, out rayhit))
            {
                Vector3 localPointEnd = this.gameObject.transform.InverseTransformPoint(rayhit.point);

                var pixelSNStart = LocalPointToPixelSN(localPointStart);
                var pixelSNEnd = LocalPointToPixelSN(localPointEnd);

                ToolTipManager.Instance().ShowDistanceInfo(() =>
                {
                    var dist = Vector3.Distance(helper.GetPointData(pixelSNEnd), helper.GetPointData(pixelSNStart));
                    return dist;
                }, localPointStart, localPointEnd, this.gameObject);
            }
        }
    }

    protected virtual void OnMouseExit()
    {

    }

    protected virtual void OnLeftMouseButtonClick(Vector3 localPoint)
    {
        Vector2Int sn = LocalPointToPixelSN(localPoint);
        ToolTipManager.Instance().ShowPixelInfo(() =>
        {
            var data = helper.GetBufferData(sn);

            return ToolTipFormat(sn, data);
        }, localPoint, this.gameObject);
    }

    private Vector2Int LocalPointToPixelSN(Vector3 localPoint)
    {
        var newPoint = localPoint + new Vector3(0.5f, -0.5f, 0);
        newPoint = new Vector3(newPoint.x, -newPoint.y, newPoint.z);

        //再将本地坐标转换为像素坐标
        //(1.0 / comm.PixelColumn)表示一个像素占的大小
        Vector2Int sn = new Vector2Int((int)(newPoint.x / (1.0 / comm.PixelWidth)), (int)(newPoint.y / (1.0 / comm.PixelHeight)));
        return sn;
    }

    protected virtual string ToolTipFormat(Vector2Int sn, float value)
    {
        return "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Vector3 localPoint = this.gameObject.transform.InverseTransformPoint(
                eventData.pointerCurrentRaycast.worldPosition);
            OnLeftMouseButtonClick(localPoint);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.dragging && eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightMouseButtonDrag(eventData.delta);
        }
        else if (eventData.dragging && eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("LeftOnDrag");


        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //这里是左键拖拽之前的按压点，也需要一个PixelInfo，并且是Fix类型的
        //不用eventData的坐标是因为有DragThreshold，即使设置为false也会有一个阈值偏差
        //拖拽完成后会有一个鼠标抬起的动作会触发OnPointerClick事件，所以OnEndDrag省略了
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Vector3 localPoint = this.gameObject.transform.InverseTransformPoint(
               eventData.pointerPressRaycast.worldPosition);
            OnLeftMouseButtonClick(localPoint);
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        OnMouseScroll(eventData.scrollDelta);
    }
}
