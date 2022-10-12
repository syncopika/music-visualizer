Shader "Unlit/Ripple"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Density("Density", float) = 100
        _Strength("Strength", float) = 2
        _Brightness("Brightness", float) = 0.3
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        //Tags { "RenderType" = "Opaque" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float2 center;// = float2(0.5, 0.5);
            float4 color;// = float4(0.3, 0.6, 1, 0.8);
            float _Speed;
            float _Density;
            float _Strength;
            float _Brightness;

            // freq bin value
            float freqBinDelta;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // https://github.com/twostraws/ShaderKit/blob/main/Shaders/SHKCircleWaveBlended.fsh
                // maybe helpful? https://stackoverflow.com/questions/41405498/how-can-i-make-gradient-sphere-on-glsl
                float waveSpeed = -(_Time.y * _Speed * 10.0);
                //if (freqBinDelta != 0) waveSpeed *= freqBinDelta * 5.0;

                float3 brightness = float3(_Brightness, _Brightness, _Brightness);
                float pixelDistance = distance(i.uv, center);

                if (pixelDistance > 0.3) {
                    // make transparent
                    return float4(1.0, 1.0, 1.0, 0);
                }

                float3 gradientColor = float3(color.r, color.g, color.b) * brightness;

                if (freqBinDelta > 0) gradientColor.g *= freqBinDelta * 5.0;

                float colorStrength = pow(1.0 - pixelDistance, 3.0);
                colorStrength *= _Strength;

                float waveDensity = _Density * pixelDistance;
                float cosine = cos(waveSpeed + waveDensity);
                float cosAdjust = (0.5 * cosine) + 0.5;
                
                float lumi = colorStrength * (_Strength + cosAdjust);
                lumi *= 1.0 - (pixelDistance * 2.0);
                lumi = max(0.0, lumi);
                
                float3 newColor = gradientColor * lumi;
                float4 final = float4(newColor.x, newColor.y, newColor.z, lumi);

                // https://alastaira.wordpress.com/2015/08/07/unity-shadertoys-a-k-a-converting-glsl-shaders-to-cghlsl/
                float4 finalColor = lerp(col, final, lumi) * col.w;

                // make alpha be a function of distance from center
                finalColor.a = pixelDistance / 5.0;

                return finalColor;
            }
            ENDCG
        }
    }
}
