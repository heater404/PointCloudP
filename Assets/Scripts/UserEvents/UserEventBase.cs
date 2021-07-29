
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UserEventBase : MonoBehaviour, IPointerClickHandler, IDragHandler, IScrollHandler
{
    protected Communication comm;
    protected float scaleSpeed = 0.19f;//避免出现零
    protected float moveSpeed = 0.003f;
    public ShaderHelperBase Helper { get; private set; }
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        comm = GameObject.Find("Manager").GetComponent<Communication>();
        Helper = this.gameObject.GetComponent<ShaderHelperBase>();
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

    protected virtual void OnLeftMouseButtonClick(Vector3 localPoint, PixelInfoStatus status = PixelInfoStatus.Active)
    {
        ToolTipManager.Instance().ShowPixelInfo(Helper.GetBufferData, Helper.bufferDataUnit, localPoint, this.gameObject, status);
    }

    protected Vector2Int LocalPointToPixelSN(Vector3 localPoint)
    {
        var newPoint = localPoint + new Vector3(0.5f, -0.5f, 0);
        newPoint = new Vector3(newPoint.x, -newPoint.y, newPoint.z);

        //再将本地坐标转换为像素坐标
        //(1.0 / comm.PixelColumn)表示一个像素占的大小
        Vector2Int sn = new Vector2Int((int)(newPoint.x / (1.0 / comm.PixelWidth)), (int)(newPoint.y / (1.0 / comm.PixelHeight)));
        return sn;
    }

    Vector3 lastRaycastWorldPosition;
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.dragging && eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightMouseButtonDrag(eventData.delta);
        }
        else if (eventData.dragging && eventData.button == PointerEventData.InputButton.Left)
        {
            if (!eventData.pointerCurrentRaycast.isValid)
            {

            }
            else
            {
                Vector3 localPointStart = this.gameObject.transform.InverseTransformPoint(
                   eventData.pointerPressRaycast.worldPosition);

                if (Vector3.zero != eventData.pointerCurrentRaycast.worldPosition)
                {
                    lastRaycastWorldPosition = eventData.pointerCurrentRaycast.worldPosition;
                }

                Vector3 localPointEnd = this.gameObject.transform.InverseTransformPoint(
                  lastRaycastWorldPosition);

                OnLeftMouseButtonDrag(localPointStart, localPointEnd);
            }
        }
    }

    protected abstract void OnLeftMouseButtonDrag(Vector3 localPointStart, Vector3 localPointEnd);

    public void OnScroll(PointerEventData eventData)
    {
        OnMouseScroll(eventData.scrollDelta);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 localPoint = this.gameObject.transform.InverseTransformPoint(
                eventData.pointerCurrentRaycast.worldPosition);
        if (eventData.button == PointerEventData.InputButton.Left
           && eventData.clickCount == 1 && !eventData.dragging)
        {
            OnLeftMouseButtonClick(localPoint);
        }

        if (eventData.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftControl))
        {
            OnLeftControlAddLeftMouseButtonClick(LocalPointToPixelSN(localPoint));
        }
    }

    protected abstract void OnLeftControlAddLeftMouseButtonClick(Vector2Int pixelSN);
}
