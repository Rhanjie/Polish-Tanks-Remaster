Shader "PolishTanks/Ground Surface"
{
    Properties
    {
        [NoScaleOffset] _Layers ("Layers (RGB colour, A height)", 2DArray) = "" {}
        [NoScaleOffset] _IndexTex ("Index (set by GroundSurface)", 2D) = "black" {}
        _EdgeWobble ("Edge wobble (cells)", Range(0, 1)) = 0.4
        _EdgeWobbleScale ("Edge wobble frequency (per cell)", Range(0.05, 2)) = 0.5
        _HeightBlend ("Height influence", Range(0, 1)) = 0.75
        _BlendSharpness ("Blend depth", Range(0.01, 0.5)) = 0.08
        _MacroVariation ("Large scale brightness variation", Range(0, 0.3)) = 0.09
        _MacroScale ("Large scale variation size (units)", Float) = 23
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend One Zero
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half2 lightingUV : TEXCOORD1;
                float2 positionWS : TEXCOORD2;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            TEXTURE2D_ARRAY(_Layers);
            SAMPLER(sampler_Layers);
            TEXTURE2D(_IndexTex);
            float4 _IndexTex_TexelSize;
            float4 _Grid;
            float _LayerScale[16];

            CBUFFER_START(UnityPerMaterial)
                float _EdgeWobble;
                float _EdgeWobbleScale;
                float _HeightBlend;
                float _BlendSharpness;
                float _MacroVariation;
                float _MacroScale;
            CBUFFER_END

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float a = Hash(i);
                float b = Hash(i + float2(1, 0));
                float c = Hash(i + float2(0, 1));
                float d = Hash(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float Fbm(float2 p)
            {
                return ValueNoise(p) * 0.6 + ValueNoise(p * 2.13 + 17.1) * 0.3 + ValueNoise(p * 4.41 + 41.7) * 0.1;
            }

            float4 BSpline(float t)
            {
                float t2 = t * t;
                float t3 = t2 * t;
                float a = 1 - t;
                return float4(a * a * a, 3 * t3 - 6 * t2 + 4, -3 * t3 + 3 * t2 + 3 * t + 1, t3) / 6.0;
            }

            int LayerAt(int2 cell)
            {
                int2 size = int2(_IndexTex_TexelSize.zw);
                cell = clamp(cell, int2(0, 0), size - 1);
                return (int)round(LOAD_TEXTURE2D(_IndexTex, cell).r * 255.0);
            }

            half4 SampleLayer(int layer, float2 positionWS)
            {
                return SAMPLE_TEXTURE2D_ARRAY(_Layers, sampler_Layers, positionWS * _LayerScale[layer], layer);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                output.lightingUV = half2(ComputeScreenPos(output.positionCS / output.positionCS.w).xy);
                output.positionWS = TransformObjectToWorld(input.positionOS).xy;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 cell = (input.positionWS - _Grid.xy) / _Grid.z;
                float2 wobble = float2(Fbm(cell * _EdgeWobbleScale), Fbm(cell * _EdgeWobbleScale + 91.7)) * 2.0 - 1.0;
                float2 p = cell + wobble * _EdgeWobble - 0.5;
                int2 c = (int2)floor(p);
                float2 f = frac(p);
                float4 wx = BSpline(f.x);
                float4 wy = BSpline(f.y);

                int layers[4] = { -1, -1, -1, -1 };
                float weights[4] = { 0, 0, 0, 0 };
                [unroll] for (int j = 0; j < 4; j++)
                {
                    [unroll] for (int i = 0; i < 4; i++)
                    {
                        int layer = LayerAt(c + int2(i - 1, j - 1));
                        float w = wx[i] * wy[j];
                        bool placed = false;
                        [unroll] for (int k = 0; k < 4; k++)
                        {
                            if (!placed && (layers[k] == layer || layers[k] < 0))
                            {
                                layers[k] = layer;
                                weights[k] += w;
                                placed = true;
                            }
                        }
                    }
                }

                half4 samples[4];
                float scores[4];
                float best = 0;
                [unroll] for (int s = 0; s < 4; s++)
                {
                    samples[s] = weights[s] > 0 ? SampleLayer(max(layers[s], 0), input.positionWS) : half4(0, 0, 0, 0);
                    scores[s] = weights[s] * lerp(1.0, 0.25 + samples[s].a * 1.5, _HeightBlend);
                    best = max(best, scores[s]);
                }

                float threshold = best - _BlendSharpness;
                half3 color = 0;
                float total = 0;
                [unroll] for (int t = 0; t < 4; t++)
                {
                    float b = max(scores[t] - threshold, 0);
                    color += samples[t].rgb * b;
                    total += b;
                }
                color /= max(total, 1e-4);
                color *= 1.0 + (Fbm(input.positionWS / _MacroScale) * 2.0 - 1.0) * _MacroVariation;

                SurfaceData2D surfaceData;
                InputData2D inputData;
                InitializeSurfaceData(color, 1, half4(1, 1, 1, 1), half3(0, 0, 1), surfaceData);
                InitializeInputData(input.uv, input.lightingUV, inputData);
                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }
    }
}
