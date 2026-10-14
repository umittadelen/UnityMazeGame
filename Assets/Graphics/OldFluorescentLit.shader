Shader "Custom/OldFluorescentLit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        _Metallic("Metallic", Range(0,1)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1

        [Header(Emission)]
        [HDR] _EmissionColor("Emission Color", Color) = (2,2,2,1)
        _EmissionMap("Emission Mask (white = glows)", 2D) = "white" {}

        [Header(Tube Positions in Object Space)]
        [Toggle] _DebugTubes("Debug: color each tube (R G B Y)", Float) = 0
        _Tube0A("Tube 0 End A", Vector) = (-0.45, 0, -0.5, 0)
        _Tube0B("Tube 0 End B", Vector) = (-0.45, 0,  0.5, 0)
        _Tube1A("Tube 1 End A", Vector) = (-0.15, 0, -0.5, 0)
        _Tube1B("Tube 1 End B", Vector) = (-0.15, 0,  0.5, 0)
        _Tube2A("Tube 2 End A", Vector) = ( 0.15, 0, -0.5, 0)
        _Tube2B("Tube 2 End B", Vector) = ( 0.15, 0,  0.5, 0)
        _Tube3A("Tube 3 End A", Vector) = ( 0.45, 0, -0.5, 0)
        _Tube3B("Tube 3 End B", Vector) = ( 0.45, 0,  0.5, 0)

        [Header(Flicker)]
        _EventPeriod("Event Slot Length (sec, keep above 5)", Float) = 8
        _EventChance("Chance Of An Event Per Slot", Range(0,1)) = 0.6
        _FlickerRate("Flicker Speed (steps per sec)", Float) = 18
        _Hum("Constant Hum Instability", Range(0,0.3)) = 0.04

        [Header(Burnout)]
        _BurnoutPeriod("Burnout Cycle Length (sec)", Float) = 900
        _BurnoutChance("Chance A Tube Dies Per Cycle", Range(0,1)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);     SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);     SAMPLER(sampler_BumpMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half   _Metallic;
                half   _Smoothness;
                half   _BumpScale;
                half4  _EmissionColor;
                float  _DebugTubes;
                float4 _Tube0A; float4 _Tube0B;
                float4 _Tube1A; float4 _Tube1B;
                float4 _Tube2A; float4 _Tube2B;
                float4 _Tube3A; float4 _Tube3B;
                float  _EventPeriod;
                float  _EventChance;
                float  _FlickerRate;
                float  _Hum;
                float  _BurnoutPeriod;
                float  _BurnoutChance;
            CBUFFER_END

            // ---------------------------------------------------------------
            // Tiny stateless random: same inputs always give the same 0..1 value
            // ---------------------------------------------------------------
            float Hash2(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // Squared distance from point p to the segment a-b.
            // If a == b it acts as a plain point.
            float SegDist2(float3 p, float3 a, float3 b)
            {
                float3 ab = b - a;
                float t = saturate(dot(p - a, ab) / max(dot(ab, ab), 1e-8));
                float3 d = p - (a + ab * t);
                return dot(d, d);
            }

            // Which tube (0..3) is this object-space point closest to?
            int NearestTube(float3 p)
            {
                int idx = 0;
                float best = SegDist2(p, _Tube0A.xyz, _Tube0B.xyz);

                float d = SegDist2(p, _Tube1A.xyz, _Tube1B.xyz);
                if (d < best) { best = d; idx = 1; }

                d = SegDist2(p, _Tube2A.xyz, _Tube2B.xyz);
                if (d < best) { best = d; idx = 2; }

                d = SegDist2(p, _Tube3A.xyz, _Tube3B.xyz);
                if (d < best) { best = d; idx = 3; }

                return idx;
            }

            // Rapid on/off stutter. Returns 0..1 brightness for the current step.
            float Flick(float local, float seed, float slot)
            {
                float step = floor(local * _FlickerRate);
                float r  = Hash2(float2(step + slot * 7.0, seed * 5.1));
                float r2 = Hash2(float2(step * 1.7 + slot, seed * 9.3));
                return r < 0.55 ? 0.0 : lerp(0.15, 1.0, r2);
            }

            // Random glitches: flicker bursts, dips, and hard-off + restart
            float EventLevel(float seed, float time)
            {
                float tl    = time + seed * 1000.0;
                float slot  = floor(tl / _EventPeriod);
                float local = tl - slot * _EventPeriod;

                float4 h = float4(
                    Hash2(float2(slot,        seed * 57.3)),
                    Hash2(float2(slot + 11.7, seed * 31.9)),
                    Hash2(float2(slot + 23.1, seed * 17.3)),
                    Hash2(float2(slot + 5.3,  seed * 3.7)));

                if (h.x > _EventChance) return 1.0;

                float level = 1.0;

                if (h.y < 0.15)
                {
                    // hard off, stutter back on, then warm up
                    float offDur     = lerp(0.3, 1.5, h.z);
                    float stutterDur = lerp(0.4, 1.0, h.w);
                    float rampDur    = 0.6;

                    if (local < offDur)                          level = 0.0;
                    else if (local < offDur + stutterDur)        level = Flick(local, seed, slot);
                    else if (local < offDur + stutterDur + rampDur)
                        level = lerp(0.3, 1.0, (local - offDur - stutterDur) / rampDur);
                }
                else if (h.y < 0.45)
                {
                    // brief dip
                    float dur = lerp(0.1, 0.5, h.z);
                    if (local < dur) level = lerp(0.25, 0.6, h.w);
                }
                else
                {
                    // flicker burst
                    float dur = lerp(0.3, 1.2, h.z);
                    if (local < dur) level = Flick(local, seed, slot);
                }
                return level;
            }

            // Very rare: last-gasp stutter, then dead until the cycle ends
            float BurnoutLevel(float seed, float time)
            {
                float bt    = time + seed * 5000.0;
                float slot  = floor(bt / _BurnoutPeriod);
                float local = bt - slot * _BurnoutPeriod;

                float h = Hash2(float2(slot + 0.5, seed * 91.7));
                if (h > _BurnoutChance) return 1.0;

                float dieAt = _BurnoutPeriod * lerp(0.1, 0.5, Hash2(float2(slot + 3.3, seed * 13.1)));
                float gasp  = 2.5;

                if (local < dieAt)        return 1.0;
                if (local < dieAt + gasp) return Flick(local, seed, slot);
                return 0.0;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float4 tangentWS  : TEXCOORD3;
                float3 positionOS : TEXCOORD4;
                half   fogFactor  : TEXCOORD5;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 6);
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                VertexPositionInputs vp = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   vn = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS = vp.positionCS;
                OUT.positionWS = vp.positionWS;
                OUT.positionOS = IN.positionOS.xyz;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS = vn.normalWS;
                OUT.tangentWS = float4(vn.tangentWS, IN.tangentOS.w * GetOddNegativeScale());
                OUT.fogFactor = ComputeFogFactor(vp.positionCS.z);

                OUTPUT_LIGHTMAP_UV(IN.lightmapUV, unity_LightmapST, OUT.lightmapUV);
                OUTPUT_SH(OUT.normalWS, OUT.vertexSH);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // --- base surface ---
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                half3 nTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv), _BumpScale);

                float3 bitangent = IN.tangentWS.w * cross(IN.normalWS, IN.tangentWS.xyz);
                float3 normalWS = normalize(TransformTangentToWorld(
                    nTS, float3x3(IN.tangentWS.xyz, bitangent, IN.normalWS)));

                // --- which tube is this pixel on? ---
                int idx = NearestTube(IN.positionOS);

                // --- per-tube, per-fixture seed ---
                // Uses the object's world position, so every prefab copy is different.
                float3 origin = GetObjectToWorldMatrix()._m03_m13_m23;
                float seed = Hash2(float2(dot(origin, float3(12.9898, 78.233, 37.719)), (idx + 1) * 17.31));

                // --- brightness of this tube right now ---
                float time = _Time.y;
                float level = EventLevel(seed, time) * BurnoutLevel(seed, time);
                level *= 1.0 - _Hum * Hash2(float2(floor(time * 40.0), seed * 9.1));

                // --- emission ---
                half3 mask = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv).rgb;
                half3 emission = _EmissionColor.rgb * mask * level;

                // Debug view: shows which tube each pixel was assigned to
                if (_DebugTubes > 0.5)
                {
                    half3 cols[4] = { half3(1,0,0), half3(0,1,0), half3(0,0.3,1), half3(1,1,0) };
                    emission = cols[idx] * 2.0 * mask * level;
                }

                // --- lighting ---
                SurfaceData surf = (SurfaceData)0;
                surf.albedo = baseTex.rgb;
                surf.metallic = _Metallic;
                surf.specular = 0;
                surf.smoothness = _Smoothness;
                surf.normalTS = nTS;
                surf.occlusion = 1;
                surf.emission = emission;
                surf.alpha = 1;

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.positionCS = IN.positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                inputData.fogCoord = IN.fogFactor;
                inputData.vertexLighting = half3(0, 0, 0);
                inputData.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                half4 color = UniversalFragmentPBR(inputData, surf);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                color.a = 1;
                return color;
            }
            ENDHLSL
        }

        // Reuse URP Lit's passes so shadows and depth work.
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
