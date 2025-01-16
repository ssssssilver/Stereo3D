Shader "Custom/SideBySide"
{
    Properties
    {
        _LeftTex ("Left Camera Texture", 2D) = "white" {}   // 左视图纹理
        _RightTex ("Right Camera Texture", 2D) = "white" {} // 右视图纹理
    }
    SubShader
    {
        // No culling or depth testing (ideal for fullscreen effects)
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Vertex input structure
            struct appdata
            {
                float4 vertex : POSITION; // Vertex position
                float2 uv : TEXCOORD0;    // Texture coordinates
            };

            // Vertex-to-fragment structure
            struct v2f
            {
                float2 uv : TEXCOORD0;      // Texture coordinates
                float4 vertex : SV_POSITION; // Transformed vertex position
            };

            // Textures
            sampler2D _LeftTex;  // 左视图
            sampler2D _RightTex; // 右视图

            // Vertex shader
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // Transform vertex position to clip space
                o.uv = v.uv;                              // Pass UV coordinates
                return o;
            }

            // Fragment shader
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col;

                // Check if the UV.x value belongs to the left or the right side
if (i.uv.x < 0.5)
                {
                    // 左半部分，映射左摄像机纹理
                    col = tex2D(_LeftTex, float2(i.uv.x * 2.0, i.uv.y));
                }
                else
                {
                    // 右半部分，映射右摄像机纹理
                    col = tex2D(_RightTex, float2((i.uv.x - 0.5) * 2.0, i.uv.y));
                }

                return col; // Return the final color
            }
            ENDCG
        }
    }
}
