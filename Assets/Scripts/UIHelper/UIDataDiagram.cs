using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LineInfo
{

    public GameObject line;
    public Color32 color;

    public LineInfo(LineInfo info)
    {
        this.color = info.color;
        this.line = info.line;
    }

    public LineInfo(GameObject line, Color32 color)
    {
        this.color = color;
        this.line = line;
    }
}

public class UIDataDiagram : MonoBehaviour
{
    private List<Color32> colors = new List<Color32> {
        //new Color32(44, 99, 123, 255),
        //new Color32(95, 164, 159, 255),
        //new Color32(172, 218, 158, 255),
        //new Color32(142, 203, 120, 255),
        //new Color32(35, 109, 86, 255),

        //new Color32(240, 223, 152, 255),
        //new Color32(252, 198, 137, 255),
        //new Color32(230, 170, 187, 255),
        //new Color32(148, 160, 182, 255),
        //new Color32(63, 150, 195, 255),

        //new Color32(79, 197, 199, 255),
        //new Color32(151, 236, 113, 255),
        //new Color32(219, 249, 119, 255),
        //new Color32(222, 157, 224, 255),
        //new Color32(250, 110, 134, 255),

        new Color32(79, 197, 199, 255),
        new Color32(151, 236, 113, 255),
        new Color32(172, 218, 158, 255),
        //new Color32(17, 40, 214, 255),
        new Color32(94, 166, 0, 255),
        new Color32(219, 249, 119, 255),
        new Color32(222, 157, 224, 255),
        new Color32(250, 110, 134, 255),
    };
    public DD_DataDiagram DataDiagram;
    public DepthUserEvents UE;

    private float updataRate = 0.2f;
    private int m_UpdataRateCnt = 0;

    Dictionary<LineInfo, Vector2Int> m_MonitorPoints = new Dictionary<LineInfo, Vector2Int>();

    // Use this for initialization
    void Start()
    {
        UE.CaptureOnePixelContinue += ContinueUpdata;
        DataDiagram.PreDestroyLineEvent += OnLineDestroy;
    }

    // Update is called once per frame
    void Update()
    {
        //RectTransform rt = gameObject.GetComponent<RectTransform>();
        //foreach (LineInfo info in m_MonitorPoints.Keys) {
        //    DataDiagram.InputPoint(info.line,
        //        new Vector2(Time.deltaTime, PointCloud.GetCloudPointDepth(m_MonitorPoints[info])));
        //}
    }

    private void FixedUpdate()
    {
        if (m_UpdataRateCnt * Time.fixedDeltaTime < updataRate)
        {
            m_UpdataRateCnt++;
            return;
        }

        foreach (LineInfo info in m_MonitorPoints.Keys)
        {
            DataDiagram.InputPoint(info.line,
                new Vector2(m_UpdataRateCnt * Time.fixedDeltaTime, UE.Helper.GetBufferData(m_MonitorPoints[info])/1000f));
        }

        m_UpdataRateCnt = 0;
    }

    Color32 TakeOneUnusedColor()
    {

        if (colors.Count > 0)
        {
            Color32 color = colors[0];
            colors.RemoveAt(0);
            return color;
        }

        return Color.black;
    }

    void GivebackOneColor(Color32 color)
    {

        if (Color.black == color)
        {
            return;
        }

        colors.Add(color);
    }

    void AddMonitorPoint(Vector2Int sn)
    {
        Color32 color = TakeOneUnusedColor();
        GameObject line = DataDiagram.AddLine(sn.ToString(), color);
        if (null == line)
        {
            return;
        }

        m_MonitorPoints.Add(new LineInfo(line, color), sn);
    }

    void RemoveMonitorPoint(GameObject line)
    {

        LineInfo? rInfo = null;
        foreach (LineInfo info in m_MonitorPoints.Keys)
        {
            if (line == info.line)
            {
                rInfo = new LineInfo?(info);
                //GivebackOneColor(info.color);
                //m_MonitorPoints.Remove(info);
            }
        }

        if (null == rInfo)
            return;

        GivebackOneColor(rInfo.Value.color);
        m_MonitorPoints.Remove(rInfo.Value);
    }

    void ContinueUpdata(object sender, Vector2Int pixelSN)
    {
        AddMonitorPoint(pixelSN);
    }

    void OnLineDestroy(object s, DD_PreDestroyLineEventArgs e)
    {
        RemoveMonitorPoint(e.line);
    }
}
