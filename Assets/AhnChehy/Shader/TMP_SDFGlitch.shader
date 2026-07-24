Shader "Custom/TMP_SDFShaderGUIGlitch"
{
    Properties
    {
        // --- TMP 기본 프로퍼티 ---
        _MainTex            ("Font Atlas", 2D) = "white" {}
        _FaceColor          ("Face Color", Color) = (1, 1, 1, 1)
        _OutlineColor       ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth       ("Outline Thickness", Range(0, 1)) = 0
        _GradientScale      ("Gradient Scale", Float) = 10.0

        // --- Glitch 프로퍼티 ---
        _GlitchOffset       ("GlitchOffset", Range(0, 1)) = 0.05
        _GlitchOffset2      ("GlitchOffset2", Range(0, 1)) = 0.02
        _GlitchTime         ("GlitchTime", Range(0, 10)) = 1.0
        _GlitchAmount       ("GlitchAmount", Range(0, 1)) = 0.3
        _ScanLinesAmount    ("Scanlines Amount", Range(0, 1000)) = 100.0
        _ScanLinesOpacity   ("Scanlines Opacity", Range(0, 1)) = 0.1
        _ScanLinesSpeed     ("Scanlines Speed", Range(-10, 10)) = 1.0
        _SplitChannelG      ("Split Channel G Offset", Vector) = (0.01, 0, 0, 0)
        _SplitChannelB      ("Split Channel B Offset", Vector) = (-0.01, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv0          : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float4 color        : COLOR;
                float2 uv0          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _FaceColor;
                half4 _OutlineColor;
                float _OutlineWidth;
                float _GradientScale;

                float _GlitchOffset;
                float _GlitchOffset2;
                float _GlitchTime;
                float _GlitchAmount;
                float _ScanLinesAmount;
                float _ScanLinesOpacity;
                float _ScanLinesSpeed;
                float2 _SplitChannelG;
                float2 _SplitChannelB;
            CBUFFER_END

            // 랜덤 노이즈
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.color = IN.color;
                OUT.uv0 = IN.uv0;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y * _GlitchTime;

                // 1. 글리치 라인 노이즈 연산
                float blockY = floor(IN.uv0.y * 25.0);
                float noise = random(float2(blockY, floor(time * 8.0)));
                
                // 확률적으로만 글리치 발생
                float isGlitch = step(1.0 - _GlitchAmount, noise);
                float offsetX = (random(float2(time, blockY)) - 0.5) * _GlitchOffset * isGlitch;

                // 2. Base UV
                float2 uvR = IN.uv0 + float2(offsetX, 0);
                float2 uvG = uvR + _SplitChannelG * isGlitch;
                float2 uvB = uvR + _SplitChannelB * isGlitch;

                // 3. TMP SDF 알파 샘플링
                float sdfR = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvR).a;
                float sdfG = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvG).a;
                float sdfB = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvB).a;

                // SDF 선명도 공식 적용
                float alphaR = saturate((sdfR - 0.5) * _GradientScale + 0.5);
                float alphaG = saturate((sdfG - 0.5) * _GradientScale + 0.5);
                float alphaB = saturate((sdfB - 0.5) * _GradientScale + 0.5);

                // 4. Color 출력 (글자 색상 + 채널 분리 RGB 글리치)
                half4 col;
                col.r = _FaceColor.r * alphaR;
                col.g = _FaceColor.g * alphaG;
                col.b = _FaceColor.b * alphaB;
                col.a = max(alphaR, max(alphaG, alphaB)) * _FaceColor.a * IN.color.a;

                // 5. Scanline 줄무늬
                float scanline = sin((IN.uv0.y + _Time.y * _ScanLinesSpeed) * _ScanLinesAmount) * 0.5 + 0.5;
                col.rgb -= scanline * _ScanLinesOpacity * col.a;

                return col;
            }
            ENDHLSL
        }
    }

    CustomEditor "TMPro.EditorUtilities.TMP_SDFShaderGUIGlitch"
}