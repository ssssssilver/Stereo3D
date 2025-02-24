Shader "Custom/StereoImageBySbs2Col"
{
    Properties
    {
        _MainTex("MainTex (Side-by-Side)", 2D) = "white" {}
        _Columns("Columns", Float) = 1.0 // 列数，用于计算交替
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _Columns;

            fixed4 frag(v2f i) : SV_Target
            {
                float2 adjustedUv = i.uv;

                // 计算当前像素所在的列索引
                float columnIndex = floor(i.uv.x * _Columns);
                bool isEvenColumn = fmod(columnIndex, 2.0) == 0.0;

                // 根据列的奇偶性选择左图或右图
                if (isEvenColumn)
                {
                    adjustedUv.x *= 0.5; // 左图（0.0 ~ 0.5）
                }
                else
                {
                    adjustedUv.x = adjustedUv.x * 0.5 + 0.5; // 右图（0.5 ~ 1.0）
                }

                return tex2D(_MainTex, adjustedUv);
            }
            ENDCG
        }
    }
}
