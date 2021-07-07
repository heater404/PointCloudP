using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace ExtraFoundation.Components
{
    public class ColorBar : BaseMeshEffect
    {
        public RangeSlider DepthRange;
        int VertexCount = 100;
        public const float HMax = 0.8f;
        float fBottomY, fTopY, fLeftX, fRightX;
        int regionCount = 8;
        float[] markes = new float[] { 1, 2, 5 };
        List<GameObject> scaleMarks = new List<GameObject>();
        protected override void Start()
        {
            //初始化的时候新建 并且始终未这个数
            for (int i = 0; i < 10; i++)
            {
                var scaleMark = (GameObject)Instantiate(Resources.Load("Prefabs/ScaleMark", typeof(GameObject)));
                scaleMark.transform.parent = this.transform;
                scaleMarks.Add(scaleMark);
            }
        }

        void Update()
        {
            float range = DepthRange.HighValue - DepthRange.LowValue;

            var step = range / regionCount;
            int power = 0;
            while (step > 10f)
            {
                step /= 10f;
                power++;
            }

            step = NearestToTarget(markes, step) * Mathf.Pow(10, power);

            var min = Mathf.Ceil(DepthRange.LowValue / step) * step;
            var max = Mathf.Floor(DepthRange.HighValue / step) * step;

            int count = (int)((max - min) / step + 1);
            if (scaleMarks.Count > count)
            {
                for (int i = 0; i < scaleMarks.Count - count; i++)
                {
                    DestroyImmediate(scaleMarks[i]);
                }
                scaleMarks.RemoveRange(0, scaleMarks.Count - count);
            }
            else if (scaleMarks.Count < count)
            {
                for (int i = 0; i < count - scaleMarks.Count; i++)
                {
                    var scaleMark = (GameObject)Instantiate(Resources.Load("Prefabs/ScaleMark", typeof(GameObject)));
                    scaleMark.transform.parent = this.transform;
                    scaleMarks.Add(scaleMark);
                }
            }

            var scale = (fTopY - fBottomY) / (DepthRange.HighValue - DepthRange.LowValue);
            for (int i = 0; i < scaleMarks.Count; i++)
            {
                var y = fTopY - scale * (step * i + min - DepthRange.LowValue);
                scaleMarks[i].transform.localPosition = new Vector3(0, y);
                scaleMarks[i].GetComponent<Text>().text = $"-{min + step * i}mm";
            }
        }

        float NearestToTarget(float[] data, float target)
        {
            int nearestIndex = 0;
            float delta = Mathf.Abs(data[nearestIndex] - target);
            for (int i = 1; i < data.Length; i++)
            {
                if (Mathf.Abs(data[i] - target) < delta)
                {
                    delta = Mathf.Abs(data[i] - target);
                    nearestIndex = i;
                }
            }
            return data[nearestIndex];
        }



        public override void ModifyMesh(VertexHelper vh)
        {
            List<UIVertex> vertexList = new List<UIVertex>();
            vh.GetUIVertexStream(vertexList);
            fBottomY = vertexList[0].position.y;
            fTopY = vertexList[0].position.y;
            fLeftX = vertexList[0].position.x;
            fRightX = vertexList[0].position.x;
            float fYPos = 0f;
            float fXPos = 0f;

            for (int i = vertexList.Count - 1; i >= 1; --i)
            {
                //找最大最小值
                fYPos = vertexList[i].position.y;
                fXPos = vertexList[i].position.x;
                if (fYPos > fTopY)
                    fTopY = fYPos;
                else if (fYPos < fBottomY)
                    fBottomY = fYPos;

                if (fXPos > fRightX)
                    fRightX = fXPos;
                else if (fXPos < fLeftX)
                    fLeftX = fXPos;
            }
            vh.Clear();
            var colorStep = HMax / (VertexCount - 1);
            var posStep = (fTopY - fBottomY) / (VertexCount - 1);

            for (int i = 0; i < VertexCount - 1; i++)
            {
                var h0 = Mathf.Lerp(0, 1, colorStep * i);
                var color0 = Color.HSVToRGB(h0, 1, 1);

                var h1 = Mathf.Lerp(0, 1, colorStep * (i + 1));
                var color1 = Color.HSVToRGB(h1, 1, 1);

                UIVertex[] verts0 = new UIVertex[4];
                var y0 = fTopY - i * posStep;
                var y1 = y0 - posStep;
                verts0[0].position = new Vector3(fLeftX, y0);
                verts0[0].color = color0;
                verts0[1].position = new Vector3(fRightX, y0);
                verts0[1].color = color0;

                verts0[2].position = new Vector3(fRightX, y1);
                verts0[2].color = color1;
                verts0[3].position = new Vector3(fLeftX, y1);
                verts0[3].color = color1;

                vh.AddUIVertexQuad(verts0);
            }
        }
    }
}
