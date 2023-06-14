Shader "Hidden/shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        //Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                // https://github.com/syncopika/funSketch/blob/master/src/filters/saturation.js

                fixed4 col = tex2D(_MainTex, i.uv);

                float satVal = 2.1;
                float lumR = .3086;
                float lumG = .6094;
                float lumB = .0820;

                float r1 = (1 - satVal) * lumR + satVal;
                float g1 = (1 - satVal) * lumG + satVal;
                float b1 = (1 - satVal) * lumB + satVal;

                float r2 = (1 - satVal) * lumR;
                float g2 = (1 - satVal) * lumG;
                float b2 = (1 - satVal) * lumB;

                float rVal = col.r;
                float gVal = col.g;
                float bVal = col.b;

                col.r = rVal * r1 + gVal * g2 + bVal * b2;
                col.g = rVal * r2 + gVal * g1 + bVal * b2;
                col.b = rVal * r2 + gVal * g2 + bVal * b1;

                return col;
            }
            ENDCG
        }
    }
}
