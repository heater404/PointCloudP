using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Linq;

public class DepthShaderHelper : ShaderHelperBase
{
    ComputeBuffer coefficient;//�ڸ�����ά����İ뾶����£���ά��Ƭӳ�䵽��ά����ı任ϵ����
    ComputeBuffer pointsBuffer;//ӳ���Ķ�ά��Ƭ�������ά����ֵ������ϵΪֱ������ϵ
    ComputeBuffer colorBuffer;//�������ɫֵ�����ݸõ��������λ����
    ComputeBuffer confidenceBuffer;
    public PointCloud pointCloud;
    public Slider Confidences;
    protected override void Awake()
    {
        base.Awake();
        Save.onClick.AddListener(() =>
        {
            SaveRenderTexture(this.texture, "Depth");

            var camera = Camera.allCameras.First(c => (int)Mathf.Log(c.cullingMask, 2) == LayerMask.NameToLayer("PointCloud")
            && c != Camera.main);
            SaveRenderTexture(camera.activeTexture, "PointCloud");
        });

        coefficient = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 12);//Vector3
        pointsBuffer = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 12);//Vector3;
        colorBuffer = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 16);//Color;rgba�ĸ�float���
        confidenceBuffer = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 4);
        bufferDataUnit = "mm";
    }

    protected override void Start()
    {
        Shader.SetBuffer(kernel, "coe", coefficient);
        Shader.SetBuffer(kernel, "points", pointsBuffer);
        Shader.SetBuffer(kernel, "colors", colorBuffer);
        Shader.SetBuffer(kernel, "confidence", confidenceBuffer);
        Shader.SetFloat("hMax", DepthColorBar.HMax);
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
                SetConfidenceBuffer();
                Dispatch(depth);
            }
            else
            {
                yield return null;
            }
        }
    }

    private void SetConfidenceBuffer()
    {
        float[] confidence;
        if (comm.ConfidenceFrames.TryDequeue(out confidence))
            confidenceBuffer.SetData(confidence);

        Shader.SetFloat("confidenceT", Confidences.value);
    }

    public override Vector3 GetPointData(Vector2Int position)
    {
        Vector3[] array = new Vector3[comm.PixelWidth * comm.PixelHeight];
        pointsBuffer.GetData(array);

        var x = position.x;
        var y = position.y;
        if (HorizontalMirrorToggle.isOn)
            x = comm.PixelWidth - x - 1;
        if (VerticalMirrorToggle.isOn)
            y = comm.PixelHeight - y - 1;

        var sn = comm.PixelWidth * y + x;

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
