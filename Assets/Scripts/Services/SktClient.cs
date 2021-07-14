using BinarySerialization;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class SktClient 
{
    public const UInt32 MAX_PKT_LEN = 65000;

    private readonly object socketLock = new object();

    public BlockingCollection<byte[]> RecvDatas { get; set; } = new BlockingCollection<byte[]>(3000);

    private UdpClient client = null;
    private IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);

    public bool Close()
    {
        if (client != null)
        {
            client.Close();
            client.Dispose();
        }
        client = null;
        return true;
    }

    public bool Open()
    {
        if (null == client)
        {
            try
            {
                client = new UdpClient(8998);
                client.Client.ReceiveBufferSize = 1 * 1024 * 1024;
                client.BeginReceive(RecvDataCallBack, null);
                Subscribe();
            }
            catch (SocketException se)
            {
                Debug.LogError(se.Message);
                return false;
            }
            return true;
        }
        return true;
    }

    private void RecvDataCallBack(IAsyncResult ar)
    {
        try
        {
            if (client != null)
            {
                RecvDatas.Add(client.EndReceive(ar, ref remoteEP));
            }
        }
        catch (Exception ex)
        {
            //异常处理
            Debug.LogError(ex.Message);
        }
        finally
        {
            if (client != null)
                client.BeginReceive(RecvDataCallBack, null);
        }
    }

    private int Send(byte[] data)
    {
        try
        {
            lock (socketLock)
            {
                if (null != client)
                {
                    IAsyncResult result = client.BeginSend(data, data.Length, remoteEP, null, null);
                    if (result.AsyncWaitHandle.WaitOne(1000))
                        Thread.Sleep(100);

                    return client.EndSend(result);
                }
            }
        }
        catch (Exception)
        {
            //异常处理
        }
        return 0;
    }

    private void SendDataCallBack(IAsyncResult ar)
    {
        if (ar.IsCompleted)
        {
            try
            {
                if (null != client)
                {
                    client.EndSend(ar);
                }
            }
            catch (Exception)
            {
                //异常处理
            }
        }
    }

    UInt32 pktSN = UInt32.MinValue;
    public int Send(MsgHeader msg)
    {
        if (null == client)
            return 0;

        msg.PktSN = pktSN;
        if (pktSN == UInt32.MaxValue)
            pktSN = UInt32.MinValue;
        else
            pktSN++;
        msg.TotalMsgLen = 1;
        msg.MsgSn = 0;
        msg.MsgType = msg.GetMsgType();
        msg.MsgLen = msg.GetMsgLen();
        msg.Timeout = 0xFFFFFFFF;

        return Send(BinarySerialize(msg));
    }

    private byte[] BinarySerialize(object obj)
    {
        BinarySerializer serializer = new BinarySerializer();
        using (MemoryStream ms = new MemoryStream())
        {
            serializer.Serialize(ms, obj);
            return ms.ToArray();
        } 
    }

    private void Subscribe()
    {
        List<UInt32> msgTable = new List<uint>()
        {
            0x114,
            0x116,
            0x117,
        };

        HelloRequest msg = new HelloRequest
        {
            MsgNum = (UInt32)msgTable.Count,
            MsgTable = msgTable.ToArray(),
        };

        this.Send(msg);
    }
}

public abstract class MsgHeader
{
    [Ignore]
    public const UInt32 HeaderLen = 24;
    public virtual UInt32 GetMsgLen()
    {
        return (UInt32)Marshal.SizeOf(this.GetType()) - HeaderLen;
    }

    public virtual MsgTypeE GetMsgType()
    {
        Type t = this.GetType();
        foreach (var att in t.GetCustomAttributes(true))
        {
            if (att is MsgTypeAttribute)
            {
                return (att as MsgTypeAttribute).MsgType;
            }
        }
        return 0x00;
    }

    // pkt Header
    [FieldOrder(1)]
    public uint PktSN { get; set; }

    //更改为总的包个数
    [FieldOrder(2)]
    public uint TotalMsgLen { get; set; }

    [FieldOrder(3)]
    public uint MsgSn { get; set; }

    [FieldOrder(4)]
    public MsgTypeE MsgType { get; set; }

    [FieldOrder(5)]
    public uint MsgLen { get; set; }//msgLen后面实际数据域字段长度

    [FieldOrder(6)]
    public uint Timeout { get; set; }
}

public enum MsgTypeE : UInt32
{
    HelloRequestMsgType = 0x10,
    CaptureRequestType = 0x11b,
    CaptureReplyType = 0x11c,

    ConfigAlgRequestType = 0x119,
    ConfigAlgReplyType = 0x11A,

    ConnectCameraRequestType = 0x11f,
    ConnectCameraReplyType = 0x120,

    ConfigCameraRequestType = 0x121,
    ConfigCameraReplyType = 0x122,

    WriteRegisterRequestType = 0x123,
    WriteRegisterReplyType = 0x124,

    ReadRegisterRequestType = 0x125,
    ReadRegisterReplyType = 0x126,

    LensArgsRequestType = 0x12A,
    LensArgsReplyType = 0x12B,

    DisconnectCameraRequestType = 0x12E,

    StartStreamingRequestType = 0x12f,

    StopStreamingRequestType = 0x130,
    StopStreamingReplyType = 0x136,

    UserAccessRequestType = 0x131,

    GetSysStatusRequestType = 0x132,
    GetSysStatusReplyType = 0x133,

    ConfigArithParamsRequestType = 0x137,//用于下发算法参数
    ConfigArithParamsReplyType = 0x138,//主要用与主动上报AE的结果

    ConfigVcselDriverRequestType = 0x139,//用于配置VcselDriver,里面的内容还需定义
    ConfigVcselDriverReplyType = 0x140,//暂时没有用到

    DisconnectCameraReplyType = 0x141,
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MsgTypeAttribute : Attribute
{
    public MsgTypeE MsgType;

    public MsgTypeAttribute(MsgTypeE msgType)
    {
        this.MsgType = msgType;
    }
}

[MsgType(MsgTypeE.HelloRequestMsgType)]
public class HelloRequest : MsgHeader
{
    [FieldLength(6)]
    [FieldOrder(1)]
    public string LocID { get; set; }//本地设备唯一标识符（暂未使用) 6字节

    [FieldOrder(2)]
    public UInt32 MsgNum { get; set; }//订阅Msg个数

    [FieldCount(nameof(MsgNum))]
    [FieldOrder(3)]
    public UInt32[] MsgTable { get; set; }

    [FieldOrder(4)]
    [FieldCount(1500)]
    public byte[] Reserve { get; set; }

    public override uint GetMsgLen()
    {
        return 6 + 4 + (UInt32)MsgTable.Length * 4 + 1500;
    }
}
