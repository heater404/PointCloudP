using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

public class DepthShaderHelper : TwoDShaderHelperBase
{
    ComputeBuffer coefficient;//在给定三维球体的半径情况下，二维照片映射到三维球面的变换系数表
    ComputeBuffer pointsBuffer;//映射后的二维照片各点的三维坐标值，坐标系为直角坐标系
    ComputeBuffer colorBuffer;//各点的颜色值，根据该点的输入相位决定
    public PointCloud pointCloud;

    protected override void Awake()
    {
        base.Awake();
        coefficient = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 12);//Vector3
        pointsBuffer = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 12);//Vector3;
        colorBuffer = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 16);//Color;rgba四个float组成
    }

    protected override void Start()
    {
        Shader.SetBuffer(kernel, "coe", coefficient);
        Shader.SetBuffer(kernel, "points", pointsBuffer);
        Shader.SetBuffer(kernel, "colors", colorBuffer);
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
}
