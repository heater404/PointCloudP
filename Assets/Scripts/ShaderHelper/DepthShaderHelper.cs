using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using ExtraFoundation.Components;

public class DepthShaderHelper : ShaderHelperBase
{
    ComputeBuffer coefficient;//�ڸ�����ά����İ뾶����£���ά��Ƭӳ�䵽��ά����ı任ϵ����
    ComputeBuffer pointsBuffer;//ӳ���Ķ�ά��Ƭ�������ά����ֵ������ϵΪֱ������ϵ
    ComputeBuffer colorBuffer;//�������ɫֵ�����ݸõ��������λ����
    public PointCloud pointCloud;

    protected override void Awake()
    {
        base.Awake();
        coefficient = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 12);//Vector3
        pointsBuffer = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 12);//Vector3;
        colorBuffer = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 16);//Color;rgba�ĸ�float���
    }

    protected override void Start()
    {
        Shader.SetBuffer(kernel, "coe", coefficient);
        Shader.SetBuffer(kernel, "points", pointsBuffer);
        Shader.SetBuffer(kernel, "colors", colorBuffer);
        Shader.SetFloat("hMax", ColorBar.HMax);
        coefficient.SetData(pointCloud.TransCoe);

        pointCloud.matVertex.SetBuffer("points", pointsBuffer);
        pointCloud.matVertex.SetBuffer("colors", colorBuffer);

        base.Start();
    }
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

    public override Vector3 GetPointData(Vector2Int position)
    {
        Vector3[] array = new Vector3[comm.PixelWidth * comm.PixelHeight];
        pointsBuffer.GetData(array);

        var sn = comm.PixelWidth * position.y + position.x;

        return array[sn];
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        coefficient.Release();
        pointsBuffer.Release();
        colorBuffer.Release();
    }
}
