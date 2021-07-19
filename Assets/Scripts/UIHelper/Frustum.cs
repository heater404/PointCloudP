using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Frustum : MonoBehaviour
{
    Communication comm;
    public Material material;
    PointCloud pointCloud;

    const float MaxDepth = 6500f;
    private void Awake()
    {
        comm = GameObject.Find("Manager").GetComponent<Communication>();
    }
    // Start is called before the first frame update
    void Start()
    {
        pointCloud = this.gameObject.GetComponent<PointCloud>();

        var pZero = Vector3.zero;

        //�������ؾ�����ĸ��ǵ����꣬Ĭ��������ֵΪ6500mm
        var zLeftTop = MaxDepth / pointCloud.TransCoe[0].z;
        var pLeftTop = new Vector3(zLeftTop * pointCloud.TransCoe[0].x, zLeftTop * pointCloud.TransCoe[0].y, zLeftTop);

        var zRightTop = MaxDepth / pointCloud.TransCoe[comm.PixelWidth - 1].z;
        var pRightTop = new Vector3(zRightTop * pointCloud.TransCoe[comm.PixelWidth - 1].x, zRightTop * pointCloud.TransCoe[comm.PixelWidth - 1].y, zRightTop);

        var zRightBottom = MaxDepth / pointCloud.TransCoe[comm.PixelWidth * comm.PixelHeight - 1].z;
        var pRightBottom = new Vector3(zRightBottom * pointCloud.TransCoe[comm.PixelWidth * comm.PixelHeight - 1].x,
            zRightBottom * pointCloud.TransCoe[comm.PixelWidth * comm.PixelHeight - 1].y, zRightBottom);

        var zLeftBottom = MaxDepth / pointCloud.TransCoe[comm.PixelWidth * (comm.PixelHeight - 1)].z;
        var pLeftBottom = new Vector3(zLeftBottom * pointCloud.TransCoe[comm.PixelWidth * (comm.PixelHeight - 1)].x,
            zLeftBottom * pointCloud.TransCoe[comm.PixelWidth * (comm.PixelHeight - 1)].y, zLeftBottom);

        var Top = Mathf.Max(pLeftTop.y, pRightTop.y);
        var Bottom = Mathf.Min(pLeftBottom.y, pRightBottom.y);
        var Left = Mathf.Min(pLeftTop.x, pLeftBottom.x);
        var Right = Mathf.Max(pRightTop.x, pRightBottom.x);
        var MaxZ = Mathf.Max(new float[] { zLeftTop, zRightTop, zRightBottom, zLeftBottom });
        //�����Ӿ������ĵ�����
        //var center = (this.pLeftTop + pRightBottom) / 2 / 2;

        //����BoxCollider��Size��
        //����MeshCollider��ԭ������Ϊ�����Զ����Mesh��������Point����LineStrip,
        //��Щ����û�л�С���ǣ���֧��Collider
        GetComponent<BoxCollider>().center = new Vector3((Left + Right) / 2, (Top + Bottom) / 2, (MaxZ + pZero.z) / 2);
        GetComponent<BoxCollider>().size = new Vector3(Mathf.Abs(Right - Left),
            Mathf.Abs(Top - Bottom), Mathf.Abs(MaxZ - pZero.z));

        Mesh frustumMesh = new Mesh() { name = "Frustum" };
        GetComponent<MeshFilter>().mesh = frustumMesh;
        Vector3[] vectices = new Vector3[5];
        vectices[0] = pZero;
        vectices[1] = pLeftTop;
        vectices[2] = pRightTop;
        vectices[3] = pRightBottom;
        vectices[4] = pLeftBottom;

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

        ////����Ҫ�ѵ�ǰ�����XY���걣��һ��
        //foreach (var cam in Camera.allCameras)
        //{
        //    if (cam.cullingMask == 1 << LayerMask.NameToLayer("PointCloud"))
        //    {
        //        cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
        //        break;
        //    }
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }
}
