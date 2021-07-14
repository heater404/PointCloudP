using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DepthColorBar : ColorBarBase
{
    public const float HMax = 0.8f;

    protected override void Start()
    {
        base.Start();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        base.ModifyMesh(vh);
    }

    protected override string MarkFormat(float value)
    {
        return $"{value}-";
    }

    protected override Color GetColor(float i)
    {
        var colorStep = HMax / (base.VertexCount - 1);
        var h = Mathf.Lerp(0, 1, colorStep * i);
        return Color.HSVToRGB(h, 1, 1);
    }

    protected override Color MarkColor()
    {
        return Color.white;
    }
}
