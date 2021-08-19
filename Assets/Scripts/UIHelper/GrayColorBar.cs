using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class GrayColorBar : MonoBehaviour
{
    public const float VMax= 1;
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

    // Use this for initialization
    void Start()
    {
        Average lowAverage = new Average(10);
        Average heightAverage = new Average(10);

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

    // Update is called once per frame
    void Update()
    {

    }

    private Color CreateOneColor(float value, float min, float max)
    {
        float hMin = 0;

        var v = (VMax - hMin) / (max - min) * (value - min) + hMin;

        return Color.HSVToRGB(0, 0, v);
    }
}
