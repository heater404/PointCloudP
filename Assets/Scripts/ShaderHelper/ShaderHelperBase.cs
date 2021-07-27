using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ShaderHelperBase : MonoBehaviour
{
    public ComputeShader Shader; //GPU����Shader
    protected int kernel;//m_CShader��ָ����һ�����㺯����ڱ��
    public RenderTexture Texture { get; private set; }
    protected ComputeBuffer buffer;
    public Communication Comm { get; private set; }

    protected float min;
    protected float max;
    protected int frameCount = 0;
    public RangeSlider Slider;
    public Toggle AutoRangeToggle;
    public Toggle ConvergenceToggle;
    public string KernelName;
    public Toggle HorizontalMirrorToggle;
    public Toggle VerticalMirrorToggle;
    public string bufferDataUnit { get; protected set; }
    protected virtual void Awake()
    {
        if (!IsSupport())
            return;

        Comm = GameObject.Find("Manager").GetComponent<Communication>();
        Slider.OnValueChanged.AddListener((min, max) =>
        {
            OnThresholdChanged(min, max);
        });

        ///������shader����Ҫʹ�õĿռ�
        ///�������Ҿ���Ӧ��˵�Ƕ�����ܸ�׼ȷ����Ϊ��Щ�ռ䶼������CPU���ڴ��У�������GPU�У�
        kernel = Shader.FindKernel(KernelName);
        buffer = new ComputeBuffer(Comm.PixelWidth * Comm.PixelHeight, 4);//float;
        Texture = new RenderTexture(Comm.PixelWidth, Comm.PixelHeight, 24, RenderTextureFormat.ARGB32)
        {
            enableRandomWrite = true
        };
        Texture.Create();

        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material.mainTexture = Texture;
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        //��m_CShader������Ӧ������
        Shader.SetFloat("width", Comm.PixelWidth);
        Shader.SetFloat("height", Comm.PixelHeight);

        Shader.SetBuffer(kernel, "buffer", buffer);
        Shader.SetTexture(kernel, "renderTexture", Texture);

        StartCoroutine(MonitorUpdate());
    }
    protected virtual IEnumerator MonitorUpdate()
    {
        yield return null;
    }


    // Update is called once per frame
    void Update()
    {

    }

    protected void Dispatch(float[] data)
    {
        if (frameCount < 3 || AutoRangeToggle.isOn)
        {
            min = Mathf.Min(data);
            max = Mathf.Max(data);
            Slider.LowValue = min;
            Slider.HighValue = max;
            frameCount++;
        }
        if (ConvergenceToggle.isOn)
        {
            Parallel.For(0, data.Length, i =>
            {
                if (data[i] > max)
                    data[i] = max;
                else if (data[i] < min)
                    data[i] = min;
            });
        }

        //ÿ�θ��µ�ǰ���е�����ֵ�����õ㵽���ĵ�ֵ
        buffer.SetData(data);

        Shader.SetFloat("max", max);
        Shader.SetFloat("min", min);
        Shader.SetBool("horizontalMirro", HorizontalMirrorToggle.isOn);
        Shader.SetBool("verticalMirro", VerticalMirrorToggle.isOn);
        //�������㣬������ʹ�õ��߹�������
        uint x, y, z;
        Shader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
        Shader.Dispatch(kernel, Comm.PixelWidth / (int)x, Comm.PixelHeight / (int)y, 1 / (int)z);
    }

    private void OnThresholdChanged(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float[] GetBufferData()
    {
        float[] array = new float[Comm.PixelWidth * Comm.PixelHeight];
        buffer.GetData(array);
        return array;
    }

    public float GetBufferData(Vector2Int position)
    {
        float[] array = GetBufferData();
        var x = position.x;
        var y = position.y;
        if (HorizontalMirrorToggle.isOn)
            x = Comm.PixelWidth - x - 1;
        if (VerticalMirrorToggle.isOn)
            y = Comm.PixelHeight - y - 1;

        var sn = Comm.PixelWidth * y + x;

        return array[sn];
    }

    public virtual Vector3 GetPointData(Vector2Int position)
    {
        return Vector3.zero;
    }
    public static bool IsSupport()
    {
        return SystemInfo.supportsComputeShaders;
    }

    protected virtual void OnDestroy()
    {
        buffer.Release();
        Texture.Release();
    }


}
