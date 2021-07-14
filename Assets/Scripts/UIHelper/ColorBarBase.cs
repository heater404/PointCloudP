using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public abstract class ColorBarBase : BaseMeshEffect
{
    public string Unit;
    public RangeSlider Range;
    protected int VertexCount = 100;
    float fBottomY, fTopY, fLeftX, fRightX;
    const int TargetCount = 8;
    readonly float[] markIntervalTab = new float[] { 1, 2, 5 };
    List<GameObject> colorMarkTexts = new List<GameObject>();
    float markTextXPos;
    const string ColorMarkTextBaseName = "ColorMarkText";
    GameObject markTextPrefab;
    protected new void Start()
    {
        markTextPrefab = (GameObject)Resources.Load("Prefabs/ColorMarkText", typeof(GameObject));

        var selfWidth = markTextPrefab.transform.GetComponent<RectTransform>().rect.width / 2;
        var parentWidth = this.transform.GetComponent<RectTransform>().rect.width / 2;
        markTextXPos = selfWidth + parentWidth;

        InitColorMarkTexts();
    }

    protected void Update()
    {
        UpdateColorMarkText(this.Range.LowValue, this.Range.HighValue);
        UpdateColorMarkTextUnit(markTextPrefab);
    }

    private void InitColorMarkTexts()
    {
        FindExistMarkText(colorMarkTexts);
        for (int i = colorMarkTexts.Count; i < 20; i++)
        {
            var markText = (GameObject)Instantiate(markTextPrefab);
            markText.name = ColorMarkTextBaseName + i;
            markText.transform.parent = this.transform;
            markText.transform.localPosition = new Vector2(0, 0);
            markText.layer = this.gameObject.layer;
            Text text = markText.GetComponent<Text>();
            text.text = "";
            text.color = MarkColor();
            text.enabled = false;
            colorMarkTexts.Add(markText);
        }
    }

    /// <summary>
    /// 每次运行前先查询当前坐标下是否已经实例化了刻度值文本UI控件
    /// 如果已经存在，则先加入队列以待使用
    /// transform是一个迭代类型，可以迭代出其所有Child节点
    /// </summary>
    /// <param name="markTexts"></param>
    private void FindExistMarkText(List<GameObject> markTexts)
    {
        foreach (Transform t in transform)
        {
            if (Regex.IsMatch(t.gameObject.name, ColorMarkTextBaseName))
            {
                markTexts.Add(t.gameObject);
            }
        }
    }

    private void UpdateColorMarkTextUnit(GameObject prefab)
    {
        var unit = this.transform.Find("Unit")?.gameObject;
        var selfHeight = prefab.transform.GetComponent<RectTransform>().rect.height / 2;
        var y = fBottomY - selfHeight;
        if (unit == null)
        {
            unit = (GameObject)Instantiate(prefab);
            unit.name = "Unit";
            unit.transform.parent = this.transform;
            unit.layer = this.gameObject.layer;
        }
        unit.transform.localPosition = new Vector2(-markTextXPos, y);
        Text text = unit.GetComponent<Text>();
        text.text = Unit;
        text.color = MarkColor();
    }

    private float CalcMarkStep(float range, float[] markIntervals, int targetCount)
    {
        var step = range / targetCount;
        int power = 0;
        while (step > 10f)
        {
            step /= 10f;
            power++;
        }

        return NearestToTarget(markIntervalTab, step) * Mathf.Pow(10, power);
    }


    protected void UpdateColorMarkText(float low, float high)
    {
        float range = high - low;

        var depthStep = CalcMarkStep(range, markIntervalTab, TargetCount);

        var min = Mathf.Ceil(low / depthStep) * depthStep;
        var max = Mathf.Floor(high / depthStep) * depthStep;
        int count = (int)((max - min) / depthStep + 1);
        if (count > colorMarkTexts.Count)
        {
            Debug.LogWarning($"colorMarkTexts OutofRange");
        }

        var posStep = (fTopY - fBottomY) / range;
        for (int i = 0; i < colorMarkTexts.Count; i++)
        {
            Text text = colorMarkTexts[i].GetComponent<Text>();
            if (i + 1 <= count)
            {
                var y = fBottomY + posStep * (depthStep * i + min - low);
                colorMarkTexts[i].transform.localPosition = new Vector2(-markTextXPos, y);
                text.text = MarkFormat(min + depthStep * i);
                text.enabled = true;
            }
            else
            {
                text.text = "";
                text.enabled = false;
            }
        }
    }


    private void SetOneColorMarkText(GameObject colorMarkText, bool enable)
    {
        var markText = colorMarkTexts.Find(mt => mt == colorMarkText);
        if (null != markText)
        {
            markText.GetComponent<Text>().enabled = false;
        }
    }

    private void SetOneColorMarkText(int index, bool enable)
    {
        if (index <= colorMarkTexts.Count - 1)
        {
            var markText = colorMarkTexts[index];
            markText.GetComponent<Text>().enabled = false;
        }
    }

    float NearestToTarget(float[] data, float target)
    {
        int nearestIndex = 0;
        float delta = Mathf.Abs(data[nearestIndex] - target);
        for (int i = 1; i < data.Length; i++)
        {
            if (Mathf.Abs(data[i] - target) < delta)
            {
                delta = Mathf.Abs(data[i] - target);
                nearestIndex = i;
            }
        }
        return data[nearestIndex];
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);
        fBottomY = vertexList[0].position.y;
        fTopY = vertexList[0].position.y;
        fLeftX = vertexList[0].position.x;
        fRightX = vertexList[0].position.x;
        float fYPos, fXPos;

        for (int i = vertexList.Count - 1; i >= 1; --i)
        {
            //找最大最小值
            fYPos = vertexList[i].position.y;
            fXPos = vertexList[i].position.x;
            if (fYPos > fTopY)
                fTopY = fYPos;
            else if (fYPos < fBottomY)
                fBottomY = fYPos;

            if (fXPos > fRightX)
                fRightX = fXPos;
            else if (fXPos < fLeftX)
                fLeftX = fXPos;
        }
        vh.Clear();

        var posStep = (fTopY - fBottomY) / (VertexCount - 1);

        for (int i = 0; i < VertexCount - 1; i++)
        {
            var color0 = GetColor(i);
            var color1 = GetColor(i + 1);

            UIVertex[] verts0 = new UIVertex[4];
            var y0 = fBottomY + i * posStep;
            var y1 = y0 + posStep;
            verts0[0].position = new Vector3(fLeftX, y0);
            verts0[0].color = color0;
            verts0[1].position = new Vector3(fRightX, y0);
            verts0[1].color = color0;

            verts0[2].position = new Vector3(fRightX, y1);
            verts0[2].color = color1;
            verts0[3].position = new Vector3(fLeftX, y1);
            verts0[3].color = color1;

            vh.AddUIVertexQuad(verts0);
        }
    }

    protected abstract string MarkFormat(float value);
    protected abstract Color GetColor(float i);
    protected abstract Color MarkColor();
}
