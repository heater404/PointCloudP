using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrayColorBar : ColorBarBase
{
    public const float VMax= 1;
    protected override Color GetColor(float i)
    {
        var colorStep = VMax / (base.VertexCount - 1);
        var v = Mathf.Lerp(0, 1, colorStep * i);
        return Color.HSVToRGB(0, 0, v);
    }

    protected override string MarkFormat(float value)
    {
        return $"{value}-";
    }

    protected override Color MarkColor()
    {
        return Color.white;
    }
}
