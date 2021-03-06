using BinarySerialization;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Communication : MonoBehaviour
{
    SktClient client = new SktClient();
    public int PixelHeight { get; private set; } = 480;
    public int PixelWidth { get; private set; } = 640;
    public LensArgs Lens { get; private set; } = new LensArgs();
    float[] depthFrame;
    float[] grayFrame;
    float[] confidenceFrame;
    public Int32 RecvDepthCnt = 0;
    public ConcurrentQueue<float[]> DepthFrames { get; set; } = new ConcurrentQueue<float[]>();
    public ConcurrentQueue<float[]> GrayFrames { get; set; } = new ConcurrentQueue<float[]>();
    public ConcurrentQueue<float[]> ConfidenceFrames { get; set; } = new ConcurrentQueue<float[]>();

    private void Awake()
    {
        Application.targetFrameRate = 30;
        this.Open();
    }

    private void OnDestroy()
    {
        this.Close();
    }

    private float[] GetDataFromCsv(string path, out int width, out int height)
    {
        List<float> data = new List<float>();
        width = 0;
        height = 0;
        using (StreamReader sr = new StreamReader(path))
        {
            do
            {
                var strs = sr.ReadLine();
                if (strs != null)
                {
                    var datas = strs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    width = datas.Length;
                    height++;
                    for (int i = 0; i < datas.Length; i++)
                    {
                        data.Add(float.Parse(datas[i]));
                    }
                }
            } while (!sr.EndOfStream);
        }
        return data.ToArray();
    }

    private Communication()
    {
        GetCommandLineArgs();

        depthFrame = new float[PixelWidth * PixelHeight];
        grayFrame = new float[PixelWidth * PixelHeight];
        confidenceFrame = new float[PixelWidth * PixelHeight];
    }
    /// <summary>
    /// ??????????  -popupwindow maxColumn*maxRowNum_Fx_Fy_Cx_Cy_sitaZoom 480*360_0_0_0_0
    /// </summary>
    private void GetCommandLineArgs()
    {
        string[] args = Environment.GetCommandLineArgs();

        if (args.Length == 3)
        {
            args = args[2].Split(new char[] { '-', '*', '_' }, StringSplitOptions.RemoveEmptyEntries);
            PixelWidth = int.Parse(args[0]);
            PixelHeight = int.Parse(args[1]);

            Lens.Fx = float.Parse(args[2]);
            Lens.Fy = float.Parse(args[3]);
            Lens.Cx = float.Parse(args[4]);
            Lens.Cy = float.Parse(args[5]);
            Lens.SitaZoom = float.Parse(args[6]);
            Debug.Log($"Recv CommandLineArgs: maxColumn:{PixelWidth} maxRowNum:{PixelHeight} fx_fy_cx_cy:{Lens.Fx}_{Lens.Fy}_{Lens.Cx}_{Lens.Cy}_{Lens.SitaZoom}");
        }
    }

    public void Open()
    {
        if (client.Open())
        {
            Debug.Log("sktClient open");
            Task.Run(() =>
            {
                UInt32 desiredPktSN = 0;
                UInt32 desiredMsgSn = 0;
                UInt32 receivedPixelLen = 0;
                byte[] data;
                while (true)
                {
                    if (client.RecvDatas.TryTake(out data, -1))
                    {
                        RecvOnePkt(data, ref desiredPktSN, ref desiredMsgSn, ref receivedPixelLen);
                    }
                }
            });
        }
    }

    public void Close()
    {
        client?.Close();
    }

    private void RecvOnePkt(byte[] data, ref UInt32 desiredPktSN, ref UInt32 desiredMsgSn, ref UInt32 receivedPixelLen)
    {
        var msgType = BitConverter.ToUInt32(data, 12);
        MsgHeader msg = BinaryDeserialize(data, typeof(ImageData));

        switch (msgType)
        {
            case 0x114://??????
                var ret = FilterInvalidImageData((ImageData)msg, 1, ref confidenceFrame, ref desiredPktSN, ref desiredMsgSn, ref receivedPixelLen);
                if (ret == 0)
                {
                    //Debug.Log("recv done confidence");
                    ConfidenceFrames.Enqueue(confidenceFrame);
                }
                break;
            case 0x116://????
                ret = FilterInvalidImageData((ImageData)msg, 2, ref depthFrame, ref desiredPktSN, ref desiredMsgSn, ref receivedPixelLen);
                if (ret == 0)
                {
                    Interlocked.Increment(ref RecvDepthCnt);
                    DepthFrames.Enqueue(depthFrame);
                    //Debug.Log($"recv done depth:{DepthFrames.Count}");
                }
                break;
            case 0x117://????
                ret = FilterInvalidImageData((ImageData)msg, 2, ref grayFrame, ref desiredPktSN, ref desiredMsgSn, ref receivedPixelLen);
                if (ret == 0)
                {
                    //Debug.Log("recv done gray");
                    GrayFrames.Enqueue(grayFrame);
                }
                break;
            default:
                Debug.LogWarning("Unknow MsgType");
                break;
        }
    }

    private int FilterInvalidImageData(ImageData data, UInt32 pixelWidth, ref float[] frame, ref UInt32 desiredPktSN, ref UInt32 desiredMsgSn, ref UInt32 receivedPixelLen)
    {
        if (data.MsgSn == 0)
        {
            desiredPktSN = data.PktSN;
            desiredMsgSn = 0;
        }

        if (data.PktSN != desiredPktSN)//?????????????? ????????????????
        {
            receivedPixelLen = 0;
            //Debug.LogWarning("wrong desiredPktSN");
            return -1;
        }

        if (data.MsgSn != desiredMsgSn)//?????????? ????????????????
        {
            //Debug.LogWarning("wrong desiredMsgSn");
            receivedPixelLen = 0;
            return -2;
        }

        //??????????
        if (receivedPixelLen + (data.MsgLen / pixelWidth) > PixelWidth * PixelHeight)
        {
            //Debug.LogWarning("wrong MsgLen");
            receivedPixelLen = 0;
            return -3;
        }

        for (int i = 0; i < data.MsgLen / pixelWidth; i++)
        {
            if (pixelWidth == 1)
                frame[i + receivedPixelLen] = data.Image[i];
            else
                frame[i + receivedPixelLen] = BitConverter.ToUInt16(data.Image, (int)(i * pixelWidth));
        }

        receivedPixelLen += (data.MsgLen / pixelWidth);
        if (receivedPixelLen == PixelWidth * PixelHeight)//recv done
        {
            desiredPktSN++;
            desiredMsgSn = 0;
            receivedPixelLen = 0;
            return 0;
        }
        else//??????????????
        {
            desiredMsgSn++;
        }

        /* need to recv more */
        return 1;
    }


    private MsgHeader BinaryDeserialize(byte[] data, Type type)
    {
        BinarySerializer serializer = new BinarySerializer();
        return (MsgHeader)serializer.Deserialize(data, type);
    }
}

public class ImageData : MsgHeader
{
    [FieldOrder(1)]
    [FieldLength(nameof(MsgLen))]
    public byte[] Image { get; set; }
}

public class LensArgs
{
    public float Fx { get; set; } = 528f;
    public float Fy { get; set; } = 528f;
    public float Cx { get; set; } = 320f;
    public float Cy { get; set; } = 240f;
    public float SitaZoom { get; set; } = 1;
}
