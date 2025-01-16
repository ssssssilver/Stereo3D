Shader "Custom/TopBottom"
{
    Properties
    {
        _LeftTex ("Top Texture (Left Camera)", 2D) = "white" {}   // 上半部分纹理（左视图）
        _RightTex ("Bottom Texture (Right Camera)", 2D) = "white" {} // 下半部分纹理（右视图）
    }
    SubShader
    {
        // No culling or depth testing
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
            sampler2D _LeftTex;  // 上半部分视图纹理
            sampler2D _RightTex; // 下半部分视图纹理

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

                // Check if the UV.y value belongs to the top or bottom half
                if (i.uv.y > 0.5)
                {
                    // Top half of the screen, sample from _LeftTex
                    col = tex2D(_LeftTex, float2(i.uv.x, (i.uv.y - 0.5) * 2.0));
                }
                else
                {
                    // Bottom half of the screen, sample from _RightTex
                    col = tex2D(_RightTex, float2(i.uv.x, i.uv.y * 2.0));
                }

                return col; // Return the final color
            }
            ENDCG
        }
    }
}
