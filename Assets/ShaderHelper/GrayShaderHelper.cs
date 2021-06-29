using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class GrayShaderHelper : TwoDShaderHelperBase
{
    protected override IEnumerator MonitorUpdate()
    {
        float[] data;
        while (true)
        {
            if (comm.GrayFrames.TryDequeue(out data))
                Dispatch(data);
            else
                yield return null;
        }
    }
}
