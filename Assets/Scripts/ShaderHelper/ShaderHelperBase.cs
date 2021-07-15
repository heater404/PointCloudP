using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ShaderHelperBase : MonoBehaviour
{
    public ComputeShader Shader; //GPU����Shader
    protected int kernel;//m_CShader��ָ����һ�����㺯����ڱ��
    public RenderTexture texture { get; private set; }//��λͼ��Ⱦʹ�õ�����������������ɫ�ɸõ����λֵ����
    public ComputeBuffer buffer;//����ĸ��㵱ǰ�����ĵľ���ֵ�����ľ����������λ�ã�������ɼ��ĸ������λֵ����ת��Ϊ����ֵ���������Ϣ
    protected Communication comm;

    protected float min;
    protected float max;
    protected int frameCount = 0;
    public RangeSlider Slider;
    public Toggle AutoRangeToggle;
    public string KernelName;
    public string bufferDataUnit { get; protected set; }
    protected virtual void Awake()
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
    protected virtual void Start()
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
        if (frameCount < 3 || AutoRangeToggle.isOn)
        {
            min = Mathf.Min(data);
            max = Mathf.Max(data);
            Slider.LowValue = min;
            Slider.HighValue = max;
            frameCount++;
        }

        Parallel.For(0, data.Length, i =>
          {
              if (data[i] > max)
                  data[i] = max;
              else if (data[i] < min)
                  data[i] = min;
          });

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
        texture.Release();
    }
}
