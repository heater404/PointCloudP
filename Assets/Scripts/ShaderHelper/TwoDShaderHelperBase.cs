using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TwoDShaderHelperBase : MonoBehaviour
{
    public ComputeShader Shader; //GPU����Shader
    int kernel;//m_CShader��ָ����һ�����㺯����ڱ��
    RenderTexture texture;//��λͼ��Ⱦʹ�õ�����������������ɫ�ɸõ����λֵ����
    ComputeBuffer buffer;//����ĸ��㵱ǰ�����ĵľ���ֵ�����ľ����������λ�ã�������ɼ��ĸ������λֵ����ת��Ϊ����ֵ���������Ϣ
    protected Communication comm;

    float min;
    float max;
    bool firstFrame = true;
    public RangeSlider Slider;
    public Toggle AutoRangeToggle;
    public string KernelName;
    void Awake()
    {
        if (!IsSupport())
            return;

        comm = GameObject.Find("Manager").GetComponent<Communication>();
        Slider.OnValueChanged.AddListener((min, max) =>
        {
            OnThresholdChanged(min, max);
        });

        ///������shader����Ҫʹ�õĿռ�
        ///�������Ҿ���Ӧ��˵�Ƕ�����ܸ�׼ȷ����Ϊ��Щ�ռ䶼������CPU���ڴ��У�������GPU�У�
        kernel = Shader.FindKernel(KernelName);
        buffer = new ComputeBuffer(comm.PixelWidth * comm.PixelHeight, 4);//float;
        texture = new RenderTexture(comm.PixelWidth, comm.PixelHeight, 24, RenderTextureFormat.ARGB32)
        {
            enableRandomWrite = true
        };
        texture.Create();

        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material.mainTexture = texture;
    }
    // Start is called before the first frame update
    void Start()
    {
        //��m_CShader������Ӧ������
        Shader.SetFloat("width", comm.PixelWidth);
        Shader.SetFloat("height", comm.PixelHeight);

        Shader.SetBuffer(kernel, "buffer", buffer);
        Shader.SetTexture(kernel, "renderTexture", texture);

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
        if (firstFrame || AutoRangeToggle.isOn)
        {
            min = Mathf.Min(data);
            max = Mathf.Max(data);
            Slider.LowValue = min;
            Slider.HighValue = max;
            firstFrame = false;
        }
        //ÿ�θ��µ�ǰ���е�����ֵ�����õ㵽���ĵ�ֵ
        buffer.SetData(data);

        Shader.SetFloat("max", max);
        Shader.SetFloat("min", min);

        //�������㣬������ʹ�õ��߹�������
        uint x, y, z;
        Shader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
        Shader.Dispatch(kernel, comm.PixelWidth / (int)x, comm.PixelHeight / (int)y, 1 / (int)z);
    }

    private void OnThresholdChanged(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float[] GetBufferData()
    {
        float[] array = new float[comm.PixelWidth * comm.PixelHeight];
        buffer.GetData(array);
        return array;
    }

    public float GetBufferData(Vector2Int position)
    {
        float[] array = new float[comm.PixelWidth * comm.PixelHeight];
        buffer.GetData(array);

        var sn = comm.PixelWidth * position.y + position.x;

        return array[sn];
    }


    public static bool IsSupport()
    {
        return SystemInfo.supportsComputeShaders;
    }

    private void OnDestroy()
    {
        buffer.Release();
        texture.Release();
    }
}
