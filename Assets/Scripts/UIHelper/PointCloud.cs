using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloud : MonoBehaviour
{
    Communication comm;
    private const int MaxPointNumPerGroup = 65000;
    private int totalPointNum;
    private int pointNumPerGroup = MaxPointNumPerGroup;
    public Vector3[] points { get; set; }
    public Vector3[] TransCoe { get; private set; }
    private Color[] colors;
    private Mesh[] meshes;
    private GameObject[] groups;
    public Material matVertex;
    private void Awake()
    {
        comm = GameObject.Find("Manager").GetComponent<Communication>();
        totalPointNum = comm.PixelHeight * comm.PixelWidth;
        //����һ�����Ƶ�mesh group�ж��ٸ���
        //��pointNumPerGroup�޷���2�����������������
        //�����������Ԥ������ֵ����ô��ƽ����̯������mesh�����᲻���ȣ��Դ����ơ�
        pointNumPerGroup = totalPointNum;
        while (pointNumPerGroup > MaxPointNumPerGroup)
        {
            pointNumPerGroup /= 2;
        }
        points = new Vector3[totalPointNum];
        TransCoe = new Vector3[totalPointNum];
        colors = new Color[totalPointNum];
        meshes = new Mesh[Mathf.CeilToInt(totalPointNum / pointNumPerGroup)];
        groups = new GameObject[meshes.Length];
    }

    // Start is called before the first frame update
    void Start()
    {
        CalcTransCoe();
        StartCoroutine(LoadPoint());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CalcTransCoe()
    {
        for (int i = 0; i < TransCoe.Length; i++)
        {
            #region �㷨���ṩ�Ĺ�ʽXYR����
            int x = i % comm.PixelWidth;//��i�����ص�x����,���ڱ궨����������480*360�½��е�
            int y = i / comm.PixelWidth;//��i�����ص�y����
            TransCoe[i].x = comm.Lens.SitaZoom * (x - comm.Lens.Cx) / comm.Lens.Fx;
            TransCoe[i].y = comm.Lens.SitaZoom * (comm.Lens.Cy - y) / comm.Lens.Fy;
            TransCoe[i].z = Mathf.Sqrt(TransCoe[i].x * TransCoe[i].x + TransCoe[i].y * TransCoe[i].y + 1);

            /* ��ά����ļ��㹫ʽ����R��ת�����ӣ�
             * Z=R/transCoeZ
             * X=transCoeX*Z
             * Y=transCoeY*Z
             */
            #endregion

            #region �㷨���ṩ�Ĺ�ʽXYZ����
            /*
             * transCoe�ļ��㹫ʽ����
             * 
             * ��ά����ļ��㹫ʽ����R��ת�����ӣ������Rʵ������Z��
             * Z=R
             * X=transCoeX*Z
             * Y=transCoeY*Z
             */
            #endregion
        }
    }

    IEnumerator LoadPoint()
    {
        yield return null;

        InitPoints();

        yield return null;


        for (int i = 0; i < meshes.Length; i++)
        {
            InstantiateMesh(this.gameObject, groups, meshes, i, points, colors);
        }
    }

    /// <summary>
    /// ��ÿ���������ɫֵ�Ħ�ֵ����Ǹö��������ж����е�����λ��
    /// ����Ⱦʱ��ÿ��������ͨ��������ɫ��ʱ��shader������ɫֵ�Ħ�ֵ�ҵ���ǰ��Ⱦ�Ķ��������ж����е�λ��
    /// ����compute shader���������points���ҵ���λ�ö�Ӧ����ά�ռ�ֵ
    /// �����points����Ӧλ�õ�ֵ��Ϊ��ǰ�����λ��ֵ�����ж���λ����Ⱦ���������ô��ݸ�������ɫ���е�vertex���ж���仯
    /// ���ԣ���pointcloudÿ�������ʼ��ʱ��Ҫ��˳����д�ø�������ɫֵ�Ħ�ֵ����ÿ���������ά����λ��
    /// ��д���Pointcloud��ÿ�������λ�ý������ٱ仯������ʵʱ����������λ�õı仯������render shader
    /// ����Ⱦʱ��compute shader��ȡֵ���ж������Ľ�������µ�
    /// �������Խ�Լcpu�����ܿ���������ı仯ֻ���ڱ���Ⱦʱ����ʾΪ�仯�ģ���ʵ�ʵ�pointcloud��״��δ�����仯
    /// 
    /// ������pointcloud����λ��ʱ��������Ҫ����һ������
    /// �ڶ������϶�ʱ��pointcloud�ᱻ�ָ�Ϊ���mesh���涨һ��mesh���65000���㣩��
    /// ��mesh��Ⱦ�Ĺ����У�����Ⱦǰ����ĳ������û��һ��������������Ŀ��ӷ�Χ�ڣ�
    /// ����������ж��㶼�������͸�shader���д���
    /// �������ǰ��������ֱ��ʵ�����������ÿ�����λ�ã���ÿ��mesh�Ķ���λ�ö��Ἧ�г������Լ�����Ӧ�ó��ֵ�������
    /// ������Ȼ��������ȷ����������������ᵼ��ĳЩmesh�������׳��������Ұ��Χ���Ӷ���ʧ��
    /// ����Щ������ÿ֡��Ⱦʱ����shader�����ͣ�ֱ�ӱ������ˣ���
    /// ������ʵ����£���Щ�����������Ұ��Χ����Ҫ����ʾ�ģ���Ϊ����Ķ���ֵ��������ʵ�Ķ���λ��ֵ����ʵ�Ķ���λ��
    /// ֵ��compute shader���������render shaderֱ���ڶ�����Ⱦ��ʱ���compute shader����������ö��㶼û�б��͵�
    /// render shader�д������̸�����ڴ���ö���ʱ��compute shader�������Ӧλ��ֵ��
    /// ���Իᵼ�±�����ʾ��ͼ��û�б���ʾ�����������Ե���δ����ʾ��
    /// Ϊ�˱����������������x��y��zʱ����������Ϊ-100f~100f���������
    /// ������֤��ÿ��mesh�еĶ����໥�������ȷֲ���������֤����ڸ����Ƕȹ۲�ʱ
    /// ÿ��mesh��������һ���������������Ұ��Χ�ڣ��������mesh��ʧ�����⡣
    /// </summary>
    void InitPoints()
    {
        for (int i = 0; i < totalPointNum; i++)
        {
            points[i] = new Vector3(UnityEngine.Random.Range(-3300f, 3300f),
                                UnityEngine.Random.Range(-3300f, 3300f),
                                UnityEngine.Random.Range(0f, 6600f));//(float)i / zAxisScale);//Vector3.zero;
            colors[i] = Color.white;
            //colors[i].a = 1;
            ///��0.1��Ϊ��ȷ��colors[i].aΪһ������i��С��i+1��������ֹ���ھ���ԭ����
            ///colors[i].a��ֵΪһ��С��i��ֵ
            colors[i].a = i + 0.1f;
        }
    }

    void InstantiateMesh(GameObject cParent, GameObject[] g, Mesh[] m,
    int meshId, Vector3[] totalPoints, Color[] totalColors)
    {

        if (null == cParent)
            return;

        if (null == m[meshId])
        {
            m[meshId] = new Mesh();//CreateMesh(meshInd, nPoints, limitPoints);
        }

        if (null == g[meshId])
        {
            g[meshId] = new GameObject("PointCloudGroup" + meshId);
            g[meshId].AddComponent<MeshFilter>();
            g[meshId].AddComponent<MeshRenderer>();

            g[meshId].GetComponent<MeshRenderer>().material = matVertex;

            g[meshId].transform.parent = cParent.transform;
            g[meshId].layer = LayerMask.NameToLayer("PointCloud");
        }

        int nPoints = (meshId == (m.Length - 1) ? (totalPointNum - (meshId * pointNumPerGroup)) :
            (pointNumPerGroup));
        UpdataMesh(m[meshId], totalPoints, totalColors, meshId * pointNumPerGroup, nPoints);

        g[meshId].GetComponent<MeshFilter>().mesh = m[meshId];
    }

    void UpdataMesh(Mesh m, Vector3[] totalPoints, Color[] totalColors, int beginIndex, int nPoints)
    {
        if (null == m)
            return;

        Vector3[] mPoints = new Vector3[nPoints];
        Color[] mColors = new Color[nPoints];
        int[] mIndecies = new int[nPoints];

        if ((null != totalPoints) && (null != totalColors))
        {
            for (int i = 0; i < nPoints; ++i)
            {
                mPoints[i] = totalPoints[beginIndex + i];
                mColors[i] = totalColors[beginIndex + i];
                mIndecies[i] = i;
            }
        }
        else
        {
            for (int i = 0; i < nPoints; ++i)
            {
                mIndecies[i] = i;
            }
        }

        m.vertices = mPoints;
        m.colors = mColors;
        m.SetIndices(mIndecies, MeshTopology.Points, 0);
    }
}
