using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Custom2ColorBar : MonoBehaviour {

    public Slider MaxSlider;
    public Slider MinSlider;
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
        Bar.CalcOneColor = CreateOneColor;
        MaxSlider.onValueChanged.AddListener(value => Max = value);
        MinSlider.onValueChanged.AddListener(value => Min = value);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private Color CreateOneColor(float value, float min, float max)
    {
        float hMin = 0, hMax = 1f;

        var v = (hMax - hMin) / (max - min) * (value - min) + hMin;

        return Color.HSVToRGB(0, 0, v);
    }
}
