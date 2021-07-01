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

            StructuredBuffer<float3> points;//mesh�����ʵ����ά����ֵ
            StructuredBuffer<float4> colors;//mesh�����ʵ����ɫ

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
                //��ǰ��Ⱦ��vertex ��λ�ã�
                //vertex��ʵ����ά����ֵ����points�У�points��vertexһһ��Ӧ
                //������ά����ֵ�Ĺ�ϵ��PointCloud.cs��void InitPoints()��ע��
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
