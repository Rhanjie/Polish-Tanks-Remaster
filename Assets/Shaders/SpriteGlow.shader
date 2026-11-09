// Additive, unlit sprite with an HDR tint. Values above 1 feed the URP Bloom post-effect,
// which turns tracers, sparks and muzzle flashes into glowing streaks.
// Works for SpriteRenderer, TrailRenderer and ParticleSystem (all multiply in vertex color).
Shader "Polish Tanks/Sprite Glow"
{
    Properties
    {
        [MainTexture][NoScaleOffset] _MainTex ("Sprite", 2D) = "white" {}
        [HDR] _GlowColor ("Glow Color", Color) = (4, 2, 0.6, 1)
        _Intensity ("Intensity", Range(0, 10)) = 1
        _CoreWhiteness ("Core Whiteness", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Blend One One
        ZWrite Off
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            half4 _GlowColor;
            half _Intensity;
            half _CoreWhiteness;
        CBUFFER_END

        struct Attributes
        {
            float3 positionOS : POSITION;
            half4 color : COLOR;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            half4 color : COLOR;
            float2 uv : TEXCOORD0;
        };

        Varyings Vert(Attributes input)
        {
            Varyings o;
            o.positionCS = TransformObjectToHClip(input.positionOS);
            o.uv = input.uv;
            o.color = input.color;
            return o;
        }

        half4 Frag(Varyings i) : SV_Target
        {
            half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
            half mask = tex.a * i.color.a;
            // The brightest part of the texture drifts toward white, like an overexposed core.
            half core = saturate(tex.r * tex.a) * _CoreWhiteness;
            half3 glow = lerp(_GlowColor.rgb, max(_GlowColor.rgb, _GlowColor.aaa * 4.0h), core);
            half3 rgb = glow * i.color.rgb * _Intensity * mask;
            return half4(rgb, 0);
        }
        ENDHLSL

        Pass
        {
            Name "Glow2D"
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }

        Pass
        {
            Name "GlowUnlit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }

    Fallback Off
}
