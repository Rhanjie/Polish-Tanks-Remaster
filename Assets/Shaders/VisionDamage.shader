// Full-screen pass for VisionDamageFeature. Every vision block owns an equal sector around the player's tank,
// block 0 centred on the hull front and the rest following clockwise. A spoiled sector is blurred, grained,
// hazed, crossed with cracks that turn with the hull and darkened; a destroyed block leaves it almost black.
// Neighbouring sectors blend softly and a small circle around the tank stays mostly clear. While the engine
// burns, the screen edges pulse red.
Shader "Hidden/Polish Tanks/Vision Damage"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "VisionDamage"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _VisionCenter;
            float4 _VisionHull;
            float4 _VisionLossA;
            float4 _VisionLossB;

            static const float BlurRadius = 0.014;
            static const float SectorBlend = 0.2;
            static const float CrackScale = 4.5;
            static const float3 Haze = float3(0.26, 0.25, 0.21);
            static const float3 AlarmColor = float3(0.9, 0.08, 0.03);

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Grain(float2 pixel, float frame)
            {
                uint h = (uint)pixel.x * 1973u + (uint)pixel.y * 9277u + (uint)frame * 26699u;
                h = (h ^ 61u) ^ (h >> 16);
                h *= 9u;
                h ^= h >> 4;
                h *= 0x27d4eb2du;
                h ^= h >> 15;
                return h * (1.0 / 4294967295.0);
            }

            float2 Hash2(float2 p)
            {
                float n = Hash(p);
                return float2(n, Hash(p + n + 17.17));
            }

            float BlockLoss(int index, int blocks)
            {
                index += index < 0 ? blocks : 0;
                index -= index >= blocks ? blocks : 0;
                float losses[6] = { _VisionLossA.x, _VisionLossA.y, _VisionLossA.z, _VisionLossA.w, _VisionLossB.x, _VisionLossB.y };
                return losses[index];
            }

            float SectorLoss(float2 offset, int blocks)
            {
                float angle = atan2(offset.x, offset.y) + _VisionHull.x;
                float position = angle / TWO_PI * blocks + 0.5;
                position -= blocks * floor(position / blocks);
                int index = (int)floor(position);
                float t = position - index;

                float own = BlockLoss(index, blocks);
                float loss = own;
                loss = lerp(loss, (own + BlockLoss(index + 1, blocks)) * 0.5, smoothstep(1.0 - SectorBlend, 1.0, t));
                loss = lerp(loss, (own + BlockLoss(index - 1, blocks)) * 0.5, 1.0 - smoothstep(0.0, SectorBlend, t));
                return loss;
            }

            float Cracks(float2 offset)
            {
                float s = sin(_VisionHull.x);
                float c = cos(_VisionHull.x);
                float2 glass = float2(c * offset.x - s * offset.y, s * offset.x + c * offset.y) * CrackScale;
                float2 cell = floor(glass);
                float2 local = frac(glass);
                float nearest = 8.0;
                float second = 8.0;
                float2 owner = cell;
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 neighbour = float2(x, y);
                        float2 seed = neighbour + Hash2(cell + neighbour) - local;
                        float gap = dot(seed, seed);
                        if (gap < nearest)
                        {
                            second = nearest;
                            nearest = gap;
                            owner = cell + neighbour;
                        }
                        else if (gap < second)
                        {
                            second = gap;
                        }
                    }
                }
                float edge = sqrt(second) - sqrt(nearest);
                float broken = step(0.4, Hash(owner * 1.31 + 7.7));
                return (1.0 - smoothstep(0.0, 0.03, edge)) * broken;
            }

            float3 Blur(float2 uv, float radius, float aspect, float rotation)
            {
                float3 sum = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
                float2 scale = float2(radius / aspect, radius);
                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    float a = rotation + i * (TWO_PI / 8.0);
                    float ring = (i % 2 == 0) ? 1.0 : 0.5;
                    float2 tap = uv + float2(cos(a), sin(a)) * scale * ring;
                    sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, tap, 0).rgb;
                }
                return sum / 9.0;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float aspect = _VisionCenter.z;
                int blocks = (int)_VisionHull.y;
                float alarm = _VisionHull.z;

                float3 color;
                float amount = 0.0;
                float2 offset = (uv - _VisionCenter.xy) * float2(aspect, 1.0);

                if (blocks > 0)
                {
                    float loss = SectorLoss(offset, blocks);
                    float fromTank = length(offset);
                    float clearRadius = _VisionCenter.w;
                    float reach = smoothstep(clearRadius, clearRadius * 1.9, fromTank);
                    amount = loss * lerp(0.2, 1.0, reach);
                }

                if (amount > 0.001)
                {
                    float2 pixel = uv * _ScreenParams.xy;
                    float frame = floor(_Time.y * 24.0);
                    float rotation = Grain(pixel, frame + 7.0) * TWO_PI;
                    color = Blur(uv, BlurRadius * amount, aspect, rotation);

                    float luma = dot(color, float3(0.299, 0.587, 0.114));
                    color = lerp(color, luma.xxx, amount * 0.6);
                    color = lerp(color, Haze * (0.5 + luma), amount * 0.2);
                    color += Cracks(offset) * smoothstep(0.15, 0.5, amount) * 0.12;
                    color *= lerp(1.0, 0.06, pow(amount, 1.4));
                    color += (Grain(pixel, frame) - 0.5) * amount * lerp(0.1, 0.03, amount * amount);
                }
                else
                {
                    color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
                }

                if (alarm > 0.001)
                {
                    float pulse = 0.5 + 0.5 * sin(_Time.y * TWO_PI * 1.4);
                    float edge = smoothstep(0.45, 0.95, length((uv - 0.5) * float2(aspect, 1.0)) / max(aspect, 1.0) * 1.6);
                    color = lerp(color, AlarmColor, edge * alarm * (0.2 + 0.35 * pulse));
                }

                return float4(max(color, 0.0), 1.0);
            }
            ENDHLSL
        }
    }
}
