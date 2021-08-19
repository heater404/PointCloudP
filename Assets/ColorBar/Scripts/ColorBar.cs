using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorBar : Graphic
{
    public int UIVertexQuadNumBetweenTwoMarkText = 3;
    const string MarkTextNameBase = "MarkText";
    public string unit = "unknown";
    readonly float[] MarkTabs = new float[] { 1, 2, 5 };
    public Color MarkTextColor = Color.white;
    public float MarkTextWidth = 40;
    public float MarkTextHeight = 16;
    private float markTextMax = 6200;
    public Func<float, float, float, Color> CalcOneColor;
    public float MarkTextMax
    {
        get { return markTextMax; }
        set
        {
            if (value > markTextMin)
            {
                markTextMax = value;
                UpdateGeometry();//调用OnPopulateMesh
            }
        }
    }
    private float markTextMin = 100;
    public float MarkTextMin
    {
        get { return markTextMin; }
        set
        {
            if (value < markTextMax)
            {
                markTextMin = value;
                UpdateGeometry();//调用OnPopulateMesh
            }
        }
    }

    GameObject markTextPrefab;

    protected override void Start()
    {
        base.Start();
        markTextPrefab = InitMarkTextPrefab();
    }

    private GameObject InitMarkTextPrefab()
    {
        var game = (GameObject)Resources.Load("Prefabs/MarkText", typeof(GameObject));
        return game;
    }

    void CreateOneMarkText(float yPos, string msg)
    {
        string name = MarkTextNameBase + msg;
        var markText = this.transform.Find(name)?.gameObject;
        if (markText == null)
        {
            markText = (GameObject)Instantiate(markTextPrefab);
            markText.name = name;
            markText.transform.SetParent(this.transform);
        }

        var x = this.gameObject.GetComponent<RectTransform>().rect.width / 2 - MarkTextWidth / 2;
        var rectTransform = markText.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(MarkTextWidth, MarkTextHeight);
        rectTransform.anchoredPosition = new Vector2(-x, yPos);
        var text = markText.GetComponent<Text>();
        text.text = msg;
        text.color = MarkTextColor;
        text.alignment = TextAnchor.MiddleRight;
        text.enabled = true;
    }

    IEnumerator CreateMarkTextUnit(float yPos, string msg)
    {
        yield return new WaitForSeconds(0);
        CreateOneMarkText(yPos, msg);
    }

    IEnumerator CreateMarkText(float markTextYPos, float markText)
    {
        yield return new WaitForSeconds(0);
        CreateOneMarkText(markTextYPos, markText.ToString() + "-");
    }

    IEnumerator DisableAllMarkTextWithoutUnit()
    {
        yield return new WaitForSeconds(0);
        var children = this.GetComponentsInChildren<Text>();
        foreach (var child in children)
        {
            if (child.name != MarkTextNameBase + unit)
                child.enabled = false;
        }
    }

    IEnumerator DestoryAllDisabledMarkText()
    {
        yield return new WaitForSeconds(0);
        var children = this.GetComponentsInChildren<Text>();
        foreach (var child in children)
        {
            if (!child.enabled)
                DestroyImmediate(child.gameObject);
        }
    }

    private float CalcRealStartMarkText(float markText)
    {
        float basePrecision = 1;
        while (markText < basePrecision || markText > basePrecision * 10)
        {
            if (markText < basePrecision)
                basePrecision /= 10;
            else if (markText > basePrecision * 10)
                basePrecision *= 10;
            else
                break;
        }
        basePrecision /= 10;

        return Mathf.CeilToInt(markText / basePrecision) * basePrecision;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (!this.gameObject.activeInHierarchy)
            return;

        var rect = this.GetPixelAdjustedRect();
        vh.Clear();
        //ColorBar矩形的坐标位置(像素单位)
        float yMax, yMin, xMin, xMax;
        yMax = rect.yMax - MarkTextHeight / 2;
        //是因为底部需要预留单位信息，所以多加了一个空间。
        yMin = rect.yMin + MarkTextHeight / 2 + MarkTextHeight / 2;
        xMin = rect.xMin + MarkTextWidth;
        xMax = rect.xMax;

        var markTextNum = CalcMarkTextNum(MarkTextHeight, yMax - yMin);
        float markTextStep = CalcMarkTextStep(MarkTabs, markTextNum, markTextMax - markTextMin);
        var rate = (yMax - yMin) / (markTextMax - markTextMin);
        var realMarkTextMin = CalcRealStartMarkText(markTextMin);
        //添加单位
        StartCoroutine(CreateMarkTextUnit(yMin - MarkTextHeight / 2 - 5, unit));//5是预留的一个空间 避免无缝相连
        StartCoroutine(DisableAllMarkTextWithoutUnit());

        var markText = realMarkTextMin;
        while (markText <= markTextMax)
        {
            //当前刻度线
            var markTextYPos = (markText - markTextMin) * rate + yMin;

            StartCoroutine(CreateMarkText(markTextYPos, markText));

            //由于两个刻度线之间的跨度较大，只画一个矩形颜色过渡不太好，所以这里在两个刻度线之间画多个
            var uiVertexQuadStep = markTextStep / UIVertexQuadNumBetweenTwoMarkText;
            for (int i = 0; i < UIVertexQuadNumBetweenTwoMarkText; i++)
            {
                //前一个刻度线
                var markTextPre = markText - (i + 1) * uiVertexQuadStep;
                if (markTextPre < markTextMin)
                    markTextPre = markTextMin;

                var markTextYPosPre = (markTextPre - markTextMin) * rate + yMin;

                var verts = CreateUIVertexQuad(xMin, xMax, markTextYPosPre, markTextYPos, markTextPre, markText);
                vh.AddUIVertexQuad(verts);

                if (markTextPre < markTextMin)
                    break;
            }
            //下一个刻度线
            markText += markTextStep;
        }

        //跳到这里说明刻度线已经大于最大值了，超出最大刻度线的肯定不需要画出来
        //最大刻度线也不需要画出来，应该它可能不是我们想要的一个数据精度
        //但是最大刻度线对应的颜色值需要我们画出来的，就相当于最小刻度颜色值也需要画出来
        var lastmarkText = markText - markTextStep;//这是画的最大的一条刻度线
        var lastmarkTextYPos = (lastmarkText - markTextMin) * rate + yMin;

        var lastverts = CreateUIVertexQuad(xMin, xMax, lastmarkTextYPos, yMax, lastmarkText, markTextMax);
        vh.AddUIVertexQuad(lastverts);

        StartCoroutine(DestoryAllDisabledMarkText());
    }

    private UIVertex[] CreateUIVertexQuad(float xMin, float xMax,
        float yBottomPos, float yTopPos, float markBottom, float markTop)
    {
        UIVertex[] verts = new UIVertex[4];
        verts[0] = CreateOneUIVertex(new Vector2(xMin, yBottomPos), markBottom);
        verts[1] = CreateOneUIVertex(new Vector2(xMax, yBottomPos), markBottom);

        verts[2] = CreateOneUIVertex(new Vector2(xMax, yTopPos), markTop);
        verts[3] = CreateOneUIVertex(new Vector2(xMin, yTopPos), markTop);

        return verts;
    }

    private UIVertex CreateOneUIVertex(Vector2 pos, float h)
    {
        UIVertex vert = new UIVertex();
        vert.position = new Vector3(pos.x, pos.y);
        if (CalcOneColor != null)
            vert.color = CalcOneColor.Invoke(h, markTextMin, markTextMax);
        vert.uv0 = Vector2.zero;

        return vert;
    }

    /// <summary>
    /// 计算刻度线的个数，乘以3是因为给予刻度线之间的间隔
    /// </summary>
    /// <param name="markTextHeigh">一个刻度线占的高度</param>
    /// <param name="colorBarHeigh">整个轴的长度</param>
    /// <returns></returns>
    private int CalcMarkTextNum(float markTextHeigh, float colorBarHeigh)
    {
        return Mathf.FloorToInt(colorBarHeigh / (markTextHeigh * 3));
    }

    /// <summary>
    /// 寻找最佳的一个刻度步长
    /// </summary>
    /// <param name="markTab">提供备选的步长基数</param>
    /// <param name="markNum">刻度个数</param>
    /// <param name="MarkTextRange">需要跨越的整个范围</param>
    /// <returns>最佳步长</returns>
    private float CalcMarkTextStep(float[] markTab, int markNum, float MarkTextRange)
    {
        float step = MarkTextRange / (markNum > 0 ? markNum : 1);
        float markScale = 1;
        float mark = markTab[0];
        while (step < mark * markScale || step > mark * markScale * 10)
        {
            if (step < mark * markScale)
                markScale /= 10;
            else if (step > mark * markScale * 10)
                markScale *= 10;
            else
                break;
        }

        step /= markScale;
        for (int i = 1; i < markTab.Length; i++)
        {
            if (Mathf.Abs(markTab[i] - step) < Mathf.Abs(mark - step))
                mark = markTab[i];
        }

        return mark * markScale;
    }
}
