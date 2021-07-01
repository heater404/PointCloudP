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

        //�������ؾ�����ĸ��ǵ����꣬Ĭ��������ֵΪ65000mm
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

        //�����Ӿ������ĵ�����
        var center = (p1 + p3) / 2 / 2;

        //����BoxCollider��Size��
        //����MeshCollider��ԭ������Ϊ�����Զ����Mesh��������Point����LineStrip,
        //��Щ����û�л�С���ǣ���֧��Collider
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

        //����Frustum�����ĵ�����
        pointCloud.PointCloudCenter = new GameObject("PointCloudCenter");
        pointCloud.PointCloudCenter.layer = LayerMask.NameToLayer("PointCloud");
        this.gameObject.transform.SetParent(pointCloud.PointCloudCenter.transform);
        pointCloud.PointCloudCenter.transform.position = center;

        //����Ҫ�ѵ�ǰ�����XY���걣��һ��
        foreach (var cam in Camera.allCameras)
        {
            if (cam.cullingMask == 1 << LayerMask.NameToLayer("PointCloud"))
            {
                cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
                break;
            }
        }

        //���Ǵ�����һ���������岢�Ұ������������壬���������帳ֵ�����ʱ�������������Ҳ�����仯
        //��ʱ���ǻ�����Ҫ�仯֮ǰ�����꣬���Ի���Ҫ��ȥһ��ƫ��
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
