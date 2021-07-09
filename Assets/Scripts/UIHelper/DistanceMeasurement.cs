using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DistanceMeasurement : BaseMeshEffect
{
    public GameObject Child;
    Vector3 LocalStartPoint;
    Vector3 LocalEndPoint;
    Func<float> GetDistance;
    GameObject target;
    float width = 2;
    Color color = Color.white;

    public override void ModifyMesh(VertexHelper vh)
    {
        vh.Clear();
        UIVertex[] verts = new UIVertex[4];

        var start = LocalPositionToScreenPoint(LocalStartPoint);
        var end = LocalPositionToScreenPoint(LocalEndPoint);

        var offset = this.gameObject.transform.position;
        var sita = Mathf.Atan((end.y - start.y) / (start.x - end.x));
        verts[0].position = new Vector3(start.x - width / 2 * Mathf.Sin(sita), start.y - width / 2 * Mathf.Cos(sita))-offset;
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
    public void Show(Vector3 start, Vector3 end, Func<float> getDistance, GameObject target)
    {
        this.LocalStartPoint = start;
        this.LocalEndPoint = end;
        this.GetDistance = getDistance;
        this.target = target;
    }

    private Vector3 LocalPositionToScreenPoint(Vector3 localPosition)
    {
        var worldPosition = target.transform.TransformPoint(localPosition);

        return Camera.main.WorldToScreenPoint(worldPosition);
    }
    // Update is called once per frame
    void Update()
    {
        var start = LocalPositionToScreenPoint(LocalStartPoint);
        var end = LocalPositionToScreenPoint(LocalEndPoint);
        var v = start - end;
        this.gameObject.transform.position = start;
        Child.transform.localPosition = new Vector3((start.x + end.x) / 2, (start.y + end.y) / 2, 0)- this.gameObject.transform.position;
        Child.transform.eulerAngles = new Vector3(0, 0, 180 * Mathf.Atan(v.y / v.x) / Mathf.PI);
        if (GetDistance == null)
            return;
        Child.transform.GetChild(0).GetComponent<Text>().text = $"{Mathf.RoundToInt(GetDistance.Invoke())}mm";

        this.GetComponent<Image>().SetVerticesDirty();//强制刷新顶点
    }
}
