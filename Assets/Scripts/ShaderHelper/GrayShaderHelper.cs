using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class GrayShaderHelper : ShaderHelperBase
{
    protected override void Start()
    {
        Shader.SetFloat("vMax", GrayColorBar.VMax);
        base.Start();
        bufferDataUnit = "LSB";
    }

    protected override IEnumerator MonitorUpdate()
    {
        float[] data;
        while (true)
        {
            if (Comm.GrayFrames.TryDequeue(out data))
                Dispatch(data);
            else
                yield return null;
        }
    }
}
