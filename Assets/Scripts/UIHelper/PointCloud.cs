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
        //计算一个点云的mesh group有多少个点
        //若pointNumPerGroup无法被2整除，这里会有问题
        //如果点数大于预设的最大值，那么会平均分摊给两个mesh，不会不均匀，以此类推。
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
            #region 算法组提供的公式XYR类型
            int x = i % comm.PixelWidth;//第i个像素的x坐标,由于标定的数据是在480*360下进行的
            int y = i / comm.PixelWidth;//第i个像素的y坐标
            TransCoe[i].x = comm.Lens.SitaZoom * (x - comm.Lens.Cx) / comm.Lens.Fx;
            TransCoe[i].y = comm.Lens.SitaZoom * (comm.Lens.Cy - y) / comm.Lens.Fy;
            TransCoe[i].z = Mathf.Sqrt(TransCoe[i].x * TransCoe[i].x + TransCoe[i].y * TransCoe[i].y + 1);

            /* 三维坐标的计算公式（由R和转换因子）
             * Z=R/transCoeZ
             * X=transCoeX*Z
             * Y=transCoeY*Z
             */
            #endregion

            #region 算法组提供的公式XYZ类型
            /*
             * transCoe的计算公式不变
             * 
             * 三维坐标的计算公式（由R和转换因子：这里的R实际上是Z）
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
    /// 用每个顶点的颜色值的α值来标记该顶点在所有顶点中的排序位置
    /// 在渲染时，每个顶点在通过顶点着色器时，shader根据颜色值的α值找到当前渲染的顶点在所有顶点中的位置
    /// 再在compute shader计算出来的points中找到该位置对应的三维空间值
    /// 最后用points中相应位置的值作为当前顶点的位置值来进行顶点位置渲染，而不是用传递给顶点着色器中的vertex进行顶点变化
    /// 所以，在pointcloud每个顶点初始化时需要按顺序填写好各顶点颜色值的α值，及每个顶点的三维坐标位置
    /// 填写完后Pointcloud中每个顶点的位置将不会再变化，而现实时看到各顶点位置的变化是由于render shader
    /// 在渲染时从compute shader中取值进行顶点计算的结果所导致的
    /// 这样可以节约cpu的性能开销，顶点的变化只是在被渲染时被显示为变化的，而实际的pointcloud形状并未发生变化
    /// 
    /// 在设置pointcloud顶点位置时，这里需要考虑一个问题
    /// 在顶点数较多时，pointcloud会被分割为多个mesh（规定一个mesh最多65000个点），
    /// 在mesh渲染的过程中，若渲染前发现某个物体没有一个顶点落在相机的可视范围内，
    /// 则该物体所有顶点都将不会送给shader进行处理
    /// 这样若是按照正常分辨率的排列来设置每个点的位置，则每个mesh的顶点位置都会集中出现在自己本来应该出现的区域内
    /// 这样虽然是真正正确的情况，但是这样会导致某些mesh将很容易超出相机视野范围，从而消失掉
    /// （这些顶点在每帧渲染时，连shader都不送，直接被无视了），
    /// 但在真实情况下，这些点可能是在视野范围内需要被显示的（因为这里的顶点值并不是真实的顶点位置值，真实的顶点位置
    /// 值由compute shader计算出来，render shader直接在顶点渲染的时候从compute shader那里读，若该顶点都没有被送到
    /// render shader中处理，则更谈不上在处理该顶点时从compute shader那里读相应位置值，
    /// 所以会导致本该显示的图像没有被显示），但被忽略掉了未被显示。
    /// 为了避免这个现象，在设置x，y，z时，将其设置为-100f~100f的随机数，
    /// 尽量保证将每个mesh中的顶点相互交错，均匀分布，这样保证相机在各个角度观察时
    /// 每个mesh都有至少一个顶点落在相机视野范围内，避免出现mesh消失的问题。
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
            ///加0.1是为了确保colors[i].a为一个大于i且小于i+1的数，防止由于精度原因导致
            ///colors[i].a的值为一个小于i的值
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
