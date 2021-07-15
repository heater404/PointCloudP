using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ShaderHelperBase : MonoBehaviour
{
    public ComputeShader Shader; //GPU计算Shader
    protected int kernel;//m_CShader中指定的一个计算函数入口编号
    public RenderTexture texture { get; private set; }//相位图渲染使用的纹理，该纹理各点的颜色由该点的相位值决定
    public ComputeBuffer buffer;//输入的各点当前到球心的距离值，球心就是摄像机的位置，摄像机采集的各点的相位值即可转化为距离值，即深度信息
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

        ///创建各shader中需要使用的空间
        ///（这里我觉得应该说是定义可能更准确，因为这些空间都不是在CPU的内存中，而是在GPU中）
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
        //给m_CShader设置相应的数据
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

        //每次更新当前所有点的深度值，即该点到球心的值
        buffer.SetData(data);

        Shader.SetFloat("max", max);
        Shader.SetFloat("min", min);

        //启动计算，并设置使用的线管组数量
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
