// Full-screen pass for VisionDamageFeature. The screen is split into fixed, irregular cells, each owned by one
// vision block, so together the blocks cover the whole view and neighbouring cells belong to different blocks.
// A destroyed block turns its cells into opaque grey static; a field-repaired one leaves them clear but
// shattered: radial cracks, broken polygonal rings and shards that shift the image, with a slight blur. Cell
// layout comes from a per-tank seed. While the engine burns the screen edges pulse red.
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

            float _VisionAspect;
            float4 _VisionParams;
            float4 _VisionLossA;
            float4 _VisionLossB;

            static const int MaxBlocks = 6;
            static const float CellsPerHeight = 3.0;
            static const float CrackRays = 9.0;
            static const float LineWidth = 0.0017;
            static const float BlurRadius = 0.012;
            static const float ShardShift = 0.007;
            static const float3 StaticGrey = float3(0.3, 0.3, 0.29);
            static const float3 GlassLight = float3(0.86, 0.88, 0.9);
            static const float3 AlarmColor = float3(0.9, 0.08, 0.03);

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float2 Hash2(float2 p)
            {
                float n = Hash(p);
                return float2(n, Hash(p + n + 17.17));
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

            float BlockLoss(int index)
            {
                float losses[6] = { _VisionLossA.x, _VisionLossA.y, _VisionLossA.z, _VisionLossA.w, _VisionLossB.x, _VisionLossB.y };
                return losses[index];
            }

            float Line(float gap, float width)
            {
                return 1.0 - smoothstep(width * 0.35, width, gap);
            }

            float Ray(float2 id, float bin, float angle, float dist, float radius)
            {
                float rayAngle = ((bin + 0.5 + (Hash(id + bin * 0.131) - 0.5) * 0.7) / CrackRays - 0.5) * TWO_PI;
                float turn = angle - rayAngle;
                turn -= TWO_PI * round(turn / TWO_PI);
                float along = dist * cos(turn);
                float span = radius * lerp(0.6, 1.3, Hash(id + bin * 0.293));
                if (Hash(id + bin * 0.517) < 0.2)
                    return 0.0;
                if (along <= 0.0 || along > span)
                    return 0.0;
                float across = abs(dist * sin(turn) + sin(along * 95.0 + bin * 2.1) * 0.0018 * along / radius);
                return Line(across, LineWidth * lerp(1.4, 0.5, along / span));
            }

            float Cracks(float2 id, float2 offset, float radius, out float2 shard)
            {
                float dist = length(offset);
                float angle = atan2(offset.y, offset.x);
                float bin = floor((angle / TWO_PI + 0.5) * CrackRays);

                float crack = 0.0;
                [unroll]
                for (int j = -1; j <= 1; j++)
                {
                    float neighbour = bin + j;
                    neighbour -= CrackRays * floor(neighbour / CrackRays);
                    crack = max(crack, Ray(id, neighbour, angle, dist, radius));
                }

                float middle = ((bin + 0.5) / CrackRays - 0.5) * TWO_PI;
                float facing = dist * cos(angle - middle);
                float ringIndex = 0.0;
                [unroll]
                for (int k = 1; k <= 3; k++)
                {
                    float ringRadius = radius * k * 0.28 * lerp(0.85, 1.15, Hash(id + bin * 0.71 + k * 3.3));
                    float shown = step(0.45, Hash(id + bin * 1.37 + k * 5.9));
                    crack = max(crack, Line(abs(facing - ringRadius), LineWidth) * shown);
                    ringIndex += step(ringRadius, facing);
                }

                crack = max(crack, 1.0 - smoothstep(0.003, 0.011, dist));
                shard = (Hash2(id + bin * 3.1 + ringIndex * 11.7) - 0.5) * ShardShift * step(0.006, dist)
                    * (1.0 - smoothstep(radius * 0.9, radius * 1.3, dist));
                return crack;
            }

            float3 Blur(float2 uv, float radius, float aspect, float rotation)
            {
                float3 sum = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
                float2 scale = float2(radius / aspect, radius);
                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    float a = rotation + i * (TWO_PI / 8.0);
                    float ring = (i & 1) == 0 ? 1.0 : 0.5;
                    sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(cos(a), sin(a)) * scale * ring, 0).rgb;
                }
                return sum / 9.0;
            }

            float BlockOf(float2 cell, int blocks)
            {
                float stride = blocks > 2 ? 2.0 : 1.0;
                float order = cell.x + cell.y * stride;
                return order - blocks * floor(order / blocks);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float aspect = _VisionAspect;
                float2 scale = float2(aspect, 1.0);
                float seed = _VisionParams.x;
                int blocks = min((int)_VisionParams.y, MaxBlocks);
                float alarm = _VisionParams.z;
                float2 pixel = uv * _ScreenParams.xy;
                float frame = floor(_Time.y * 24.0);

                float3 color;
                float crack = 0.0;
                float2 shift = 0.0;
                float loss = 0.0;
                float glass = 0.0;

                if (blocks > 0)
                {
                    float2 position = uv * scale;
                    float2 warp = float2(
                        sin(position.y * 5.1 + seed) * 0.035 + sin(position.y * 11.3 + seed * 2.0) * 0.015,
                        sin(position.x * 4.7 + seed * 1.3) * 0.035 + sin(position.x * 12.1 + seed * 0.7) * 0.015);
                    float2 grid = (position + warp) * CellsPerHeight;
                    float2 cell = floor(grid);
                    float2 local = frac(grid);

                    float nearest = 8.0;
                    float second = 8.0;
                    float2 owner = cell;
                    float2 runnerUp = cell;
                    float2 impact = 0.5;
                    [unroll]
                    for (int y = -1; y <= 1; y++)
                    {
                        [unroll]
                        for (int x = -1; x <= 1; x++)
                        {
                            float2 neighbour = cell + float2(x, y);
                            float2 centre = float2(x, y) + 0.15 + 0.7 * Hash2(neighbour + seed);
                            float2 toCentre = centre - local;
                            float gap = dot(toCentre, toCentre);
                            if (gap < nearest)
                            {
                                second = nearest;
                                runnerUp = owner;
                                nearest = gap;
                                owner = neighbour;
                                impact = centre;
                            }
                            else if (gap < second)
                            {
                                second = gap;
                                runnerUp = neighbour;
                            }
                        }
                    }

                    float own = BlockLoss((int)BlockOf(owner, blocks));
                    float other = BlockLoss((int)BlockOf(runnerUp, blocks));
                    float border = (sqrt(second) - sqrt(nearest)) / CellsPerHeight;
                    loss = lerp((own + other) * 0.5, own, smoothstep(0.0, 0.025, border));
                    glass = saturate(own * 2.0);

                    if (own > 0.001)
                    {
                        float2 shard;
                        float2 offset = (local - impact) / CellsPerHeight;
                        crack = Cracks(owner + seed * 0.37, offset, 0.75 / CellsPerHeight, shard) * glass;
                        shift = shard * glass;
                    }
                }

                float2 sampleUv = uv + shift / scale;
                if (loss > 0.001)
                {
                    float rotation = Grain(pixel, frame + 7.0) * TWO_PI;
                    float blur = saturate(loss * 2.0) * 0.35 + smoothstep(0.5, 1.0, loss) * 0.65;
                    color = Blur(sampleUv, BlurRadius * blur, aspect, rotation);
                    float luma = dot(color, float3(0.299, 0.587, 0.114));
                    color = lerp(color, luma.xxx * 0.9 + 0.04, saturate(loss * 2.0) * 0.15);
                    float3 noise = StaticGrey + (Grain(pixel, frame) - 0.5) * 0.2;
                    color = lerp(color, noise, smoothstep(0.5, 1.0, loss));
                }
                else
                {
                    color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, sampleUv, 0).rgb;
                }

                float glint = 0.6 + 0.4 * Grain(floor(pixel / 2.0), 0.0);
                color = lerp(color, GlassLight, crack * 0.42 * glint);

                if (alarm > 0.001)
                {
                    float pulse = 0.5 + 0.5 * sin(_Time.y * TWO_PI * 1.4);
                    float rim = smoothstep(0.45, 0.95, length((uv - 0.5) * scale) / max(aspect, 1.0) * 1.6);
                    color = lerp(color, AlarmColor, rim * alarm * (0.2 + 0.35 * pulse));
                }

                return float4(max(color, 0.0), 1.0);
            }
            ENDHLSL
        }
    }
}
