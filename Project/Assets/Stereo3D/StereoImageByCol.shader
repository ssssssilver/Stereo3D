Shader "Custom/StereoImageByCol"
{
    Properties
    {
        _MainTex("MainTex",2D)="white"{}
        _LeftTex ("LeftCameraTex", 2D) = "white" { }
        _RightTex ("RightCameraTex", 2D) = "white" { }
    }
    SubShader
    {
        // No culling or depth
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
            sampler2D _LeftTex;
            sampler2D _RightTex;
            float _Columns;
            float _Rows;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col;
                if (floor(fmod(float(i.uv.x * _Columns), 2.0)) == 0.0)
                {
                   
                    col = tex2D(_LeftTex, float2(i.uv.x / 2, i.uv.y));
                }
                else
                {
                    col = tex2D(_RightTex, float2(i.uv.x / 2, i.uv.y));
                }

                return col;
            }
            ENDCG
        }
    }
}
