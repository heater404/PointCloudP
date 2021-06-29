using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

public class DepthShaderHelper : TwoDShaderHelperBase
{
    protected override IEnumerator MonitorUpdate()
    {
        float[] depth;
        while (true)
        {
            if (comm.DepthFrames.TryDequeue(out depth))
            {
                Dispatch(depth);
            }
            else
            {
                yield return null;
            }
        }
    }
}
