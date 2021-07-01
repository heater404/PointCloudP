using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Frustum : MonoBehaviour
{
    Communication comm;
    public Material material;
    PointCloud pointCloud;
    Vector3 p0;
    Vector3 p1;
    Vector3 p2;
    Vector3 p3;
    Vector3 p4;
    const float MaxDepth = 6500.0f;
    private void Awake()
    {
        comm = GameObject.Find("Manager").GetComponent<Communication>();
        pointCloud = this.gameObject.GetComponent<PointCloud>();
    }
    // Start is called before the first frame update
    void Start()
    {
        p0 = Vector3.zero;

        //计算像素矩阵的四个角的坐标，默认最大深度值为65000mm
        var z1 = MaxDepth / pointCloud.TransCoe[0].z;
        p1 = new Vector3(z1 * pointCloud.TransCoe[0].x, z1 * pointCloud.TransCoe[0].y, z1);

        var z2 = MaxDepth / pointCloud.TransCoe[comm.PixelWidth - 1].z;
        p2 = new Vector3(z2 * pointCloud.TransCoe[comm.PixelWidth - 1].x, z2 * pointCloud.TransCoe[comm.PixelWidth - 1].y, z2);

        var z3 = MaxDepth / pointCloud.TransCoe[comm.PixelWidth * comm.PixelHeight - 1].z;
        p3 = new Vector3(z3 * pointCloud.TransCoe[comm.PixelWidth * comm.PixelHeight - 1].x,
            z3 * pointCloud.TransCoe[comm.PixelWidth * comm.PixelHeight - 1].y, z3);

        var z4 = MaxDepth / pointCloud.TransCoe[comm.PixelWidth * (comm.PixelHeight - 1)].z;
        p4 = new Vector3(z4 * pointCloud.TransCoe[comm.PixelWidth * (comm.PixelHeight - 1)].x,
            z4 * pointCloud.TransCoe[comm.PixelWidth * (comm.PixelHeight - 1)].y, z4);

        //计算视景体中心点坐标
        var center = (p1 + p3) / 2 / 2;

        //计算BoxCollider的Size，
        //不用MeshCollider的原因是因为我们自定义的Mesh的类型是Point或者LineStrip,
        //这些类型没有画小三角，不支持Collider
        GetComponent<BoxCollider>().center = new Vector3((p1.x + p2.x) / 2, (p1.y + p4.y) / 2, p0.z);
        GetComponent<BoxCollider>().size = new Vector3(Mathf.Abs(p1.x - p2.x),
            Mathf.Abs(p1.y - p4.y), Mathf.Abs(p1.z - p0.z));

        Mesh frustumMesh = new Mesh() { name = "Frustum" };
        GetComponent<MeshFilter>().mesh = frustumMesh;
        Vector3[] vectices = new Vector3[5];
        vectices[0] = p0;
        vectices[1] = p1;
        vectices[2] = p2;
        vectices[3] = p3;
        vectices[4] = p4;

        int[] indexs = new int[12];
        indexs[0] = 0;
        indexs[1] = 1;
        indexs[2] = 0;
        indexs[3] = 2;
        indexs[4] = 0;
        indexs[5] = 3;
        indexs[6] = 0;
        indexs[7] = 4;
        indexs[8] = 1;
        indexs[9] = 2;
        indexs[10] = 3;
        indexs[11] = 4;

        frustumMesh.vertices = vectices;
        frustumMesh.SetIndices(indexs, MeshTopology.LineStrip, 0);

        //创建Frustum的中心点物体
        pointCloud.PointCloudCenter = new GameObject("PointCloudCenter");
        pointCloud.PointCloudCenter.layer = LayerMask.NameToLayer("PointCloud");
        this.gameObject.transform.SetParent(pointCloud.PointCloudCenter.transform);
        pointCloud.PointCloudCenter.transform.position = center;

        //还需要把当前相机的XY坐标保持一致
        foreach (var cam in Camera.allCameras)
        {
            if (cam.cullingMask == 1 << LayerMask.NameToLayer("PointCloud"))
            {
                cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
                break;
            }
        }

        //我们创建了一个中心物体并且把它当作父物体，当给父物体赋值坐标的时候，子物体的坐标也会跟随变化
        //当时我们还是想要变化之前的坐标，所以还需要减去一个偏移
        for (int i = 0; i < vectices.Length; i++)
        {
            vectices[i] -= pointCloud.PointCloudCenter.transform.position;
        }
        frustumMesh.vertices = vectices;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
