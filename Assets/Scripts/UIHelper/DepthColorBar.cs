using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class Average
{
    private int dataNum;
    List<float> data = new List<float>();
    public Average(int dataNumber)
    {
        dataNum = dataNumber;
    }
    public bool CalcAverage(float value, out float average)
    {
        average = 0;
        data.Add(value);
        if (data.Count < dataNum)
            return false;
        else
        {
            data.Remove(Mathf.Max(data.ToArray()));
            data.Remove(Mathf.Min(data.ToArray()));
            average = data.Average();
            data.RemoveAt(0);
        }
        return true;
    }
}

public class DepthColorBar : MonoBehaviour
{
    public const float HMax = 0.8f;

    public RangeSlider Slider;
    public ColorBar Bar;
    public float Max
    {
        get { return Bar.MarkTextMax; }
        set { Bar.MarkTextMax = value; }
    }

    public float Min
    {
        get { return Bar.MarkTextMin; }
        set { Bar.MarkTextMin = value; }
    }

    Average lowAverage, heightAverage;
    private float lastLow, lastHigh;
    // Use this for initialization
    void Start()
    {
        lowAverage = new Average(5);
        heightAverage = new Average(5);

        Bar.CalcOneColor = CreateOneColor;
        Slider.OnValueChanged.AddListener((l, h) =>
        {
            float low, hight;
            if (lowAverage.CalcAverage(l, out low))
                Min = low;
            else
                Min = l;

            if (heightAverage.CalcAverage(h, out hight))
                Max = hight;
            else
                Max = h;
        });
    }

    private Color CreateOneColor(float value, float min, float max)
    {
        float hMin = 0;

        var h = (HMax - hMin) / (max - min) * (value - min) + hMin;

        return Color.HSVToRGB(h, 1, 1);
    }
}
