using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DistanceMeasurement : BaseMeshEffect, IPointerClickHandler
{
    public GameObject Child;
    Vector3 localStartPoint;
    Vector3 localEndPoint;
    Func<Vector2Int, Vector3> GetPixelPosition;
    Func<Vector2Int, float> GetPixelValue;
    GameObject target;
    float width = 2;
    Color color = Color.white;
    Vector2Int startSN;
    Vector2Int endSN;
    Text text;

    protected override void Awake()
    {
        base.Awake();
        text = Child.transform.GetChild(0).GetComponent<Text>();
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(UpdateDistanceMeasurement());
    }
    public override void ModifyMesh(VertexHelper vh)
    {
        vh.Clear();
        UIVertex[] verts = new UIVertex[4];

        var start = LocalPositionToScreenPoint(localStartPoint);
        var end = LocalPositionToScreenPoint(localEndPoint);

        var offset = this.gameObject.transform.position;
        var sita = Mathf.Atan((end.y - start.y) / (start.x - end.x));
        verts[0].position = new Vector3(start.x - width / 2 * Mathf.Sin(sita), start.y - width / 2 * Mathf.Cos(sita)) - offset;
        verts[0].color = color;
        verts[0].uv0 = Vector2.zero;

        verts[1].position = new Vector3(start.x + width / 2 * Mathf.Sin(sita), start.y + width / 2 * Mathf.Cos(sita)) - offset;
        verts[1].color = color;
        verts[1].uv0 = Vector2.zero;

        verts[2].position = new Vector3(end.x + width / 2 * Mathf.Sin(sita), end.y + width / 2 * Mathf.Cos(sita)) - offset;
        verts[2].color = color;
        verts[2].uv0 = Vector2.zero;

        verts[3].position = new Vector3(end.x - width / 2 * Mathf.Sin(sita), end.y - width / 2 * Mathf.Cos(sita)) - offset;
        verts[3].color = color;
        verts[3].uv0 = Vector2.zero;

        vh.AddUIVertexQuad(verts);
    }
    public void Show(Vector3 start, Vector3 end, Func<Vector2Int, Vector3> getPixelPosition, Func<Vector2Int, float> updatePixelValue, GameObject target)
    {
        this.localStartPoint = start;
        this.localEndPoint = end;
        this.GetPixelPosition = getPixelPosition;
        this.target = target;

        this.startSN = LocalPointToPixelSN(localStartPoint);
        this.endSN = LocalPointToPixelSN(localEndPoint);

        var startP = this.transform.Find("StartPixelInfo").GetComponent<PixelInfo>();
        startP.Status = PixelInfoStatus.Fix;
        startP.Show(updatePixelValue, "mm", start, target);
        var endP = this.transform.Find("EndPixelInfo").GetComponent<PixelInfo>();
        endP.Status = PixelInfoStatus.Fix;
        endP.Show(updatePixelValue, "mm", end, target);
    }

    private Vector3 LocalPositionToScreenPoint(Vector3 localPosition)
    {
        var worldPosition = target.transform.TransformPoint(localPosition);

        return Camera.main.WorldToScreenPoint(worldPosition);
    }

    private Vector2Int LocalPointToPixelSN(Vector3 localPoint)
    {
        var comm = GameObject.Find("Manager").GetComponent<Communication>();

        var newPoint = localPoint + new Vector3(0.5f, -0.5f, 0);
        newPoint = new Vector3(newPoint.x, -newPoint.y, newPoint.z);

        //再将本地坐标转换为像素坐标
        //(1.0 / comm.PixelColumn)表示一个像素占的大小
        Vector2Int sn = new Vector2Int((int)(newPoint.x / (1.0 / comm.PixelWidth)), (int)(newPoint.y / (1.0 / comm.PixelHeight)));
        return sn;
    }

    IEnumerator UpdateDistanceMeasurement()
    {
        while (true)
        {
            var start = LocalPositionToScreenPoint(localStartPoint);
            var end = LocalPositionToScreenPoint(localEndPoint);
            var v = start - end;
            this.gameObject.transform.position = start;
            Child.transform.localPosition = new Vector3((start.x + end.x) / 2, (start.y + end.y) / 2, 0) - this.gameObject.transform.position;
            Child.transform.eulerAngles = new Vector3(0, 0, 180 * Mathf.Atan(v.y / v.x) / Mathf.PI);
            if (GetPixelPosition == null)
                yield return new WaitForSeconds(0.5f);

            var dist = Vector3.Distance(GetPixelPosition.Invoke(endSN), GetPixelPosition.Invoke(startSN));
            text.text = $"{Mathf.RoundToInt(dist)}mm";
            this.GetComponent<Image>().SetVerticesDirty();//强制刷新顶点
            yield return new WaitForSeconds(0.5f);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ToolTipManager.Instance().DestoryDistanceInfo();
        }
    }
}

