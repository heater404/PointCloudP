Shader "Custom/PointCloudShader"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma target 5.0 

            #include "UnityCG.cginc"

            StructuredBuffer<float3> points;//mesh顶点的实际三维坐标值
            StructuredBuffer<float4> colors;//mesh顶点的实际颜色

            struct appdata
            {
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                //求当前渲染点vertex （位置）
                //vertex的实际三维坐标值存在points中，points与vertex一一对应
                //顶点三维坐标值的关系见PointCloud.cs中void InitPoints()的注释
                int i = floor(v.color.a);
                o.pos = UnityObjectToClipPos(float4(points[i], 1));
                o.color = colors[i];
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
