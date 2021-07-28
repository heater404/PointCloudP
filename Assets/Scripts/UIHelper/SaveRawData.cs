using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SaveRawData : MonoBehaviour
{
    public DepthShaderHelper DepthHelper;
    public GrayShaderHelper GrayHelper;
    // Use this for initialization
    void Start()
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            Save();
        });
    }

    void Save()
    {
        string directory = $@"SnapShots/{DateTime.Now:yyyyMMdd}/{DateTime.Now:HHmmssffff}";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        //SaveDepth
        var path = directory + $"/Depth.png";
        SaveRenderTextureToPNG(DepthHelper.Texture, path);
        path = directory + $"/Depth.csv";
        SaveRawDataToCsv(DepthHelper.GetBufferData(), DepthHelper.Comm.PixelWidth, DepthHelper.Comm.PixelHeight, path);

        //SavePointCloud
        path = directory + $"/PointCloud.png";
        var camera = Camera.allCameras.First(c => (int)Mathf.Log(c.cullingMask, 2) == LayerMask.NameToLayer("PointCloud")
        && c != Camera.main);
        SaveRenderTextureToPNG(camera.activeTexture, path);
        path = directory + $"/PointCloud.csv";
        SaveRawDataToCsv(DepthHelper.GetPointData(), path);

        //SaveGray
        path = directory + $"/Gray.png";
        SaveRenderTextureToPNG(GrayHelper.Texture, path);
        path = directory + $"/Gray.csv";
        SaveRawDataToCsv(GrayHelper.GetBufferData(), GrayHelper.Comm.PixelWidth, GrayHelper.Comm.PixelHeight, path);
    }

    private void SaveRenderTextureToPNG(RenderTexture renderTexture, string path)
    {
        int width = renderTexture.width;
        int height = renderTexture.height;
        Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();
        byte[] vs = texture2D.EncodeToPNG();

        FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
        fileStream.Write(vs, 0, vs.Length);
        fileStream.Dispose();
        fileStream.Close();
    }

    private void SaveRawDataToCsv(float[] data, int width, int height, string path)
    {
        Task.Run(() =>
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int j = 0; j < height; j++)
                {
                    //写一行
                    for (int i = 0; i < width; i++)
                    {
                        sw.Write(data[j * width + i] + ",");
                    }
                    sw.WriteLine();//一行结束后换行
                }
            }
        });
    }

    private void SaveRawDataToCsv(Vector3[] data, string path)
    {
        Task.Run(() =>
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int j = 0; j < data.Length; j++)
                {
                    sw.Write(data[j].x + "," + data[j].y + "," + data[j].z);
                    sw.WriteLine();//一行结束后换行
                }
            }
        });
    }
}
