// Unlit map-symbol ink for the draped unit ribbons (atlas-cartography
// plan, Task 3). SymbolMeshBuilder's UV contract drives everything: fill
// vertices carry uv.y in [0, 1] (uv.x runs along the frontage), border
// strips carry uv.y in [2, 3] across the strip width. The fragment reads
// the band, not a submesh index, so ONE material serves both submeshes:
//  - fill: _BaseColor flat (this is a MAP symbol — no light loop), with
//    the provenance styling selected by _FillStyle (MPB per unit):
//    0 = documented, solid; 1 = inferred, UV-space diagonal hatch whose
//    gaps clip away so the ground shows through (deterministic, no
//    texture asset); 2 = RESERVED cross-hatch for Phase 11 proper's
//    contested vocabulary — nothing maps to it yet (user ruling
//    2026-07-09: two states render).
//  - border: _BaseColor darkened to a constant ink shade, weighted by
//    _BorderWeight (MPB per unit, from the unit's echelon —
//    BattleDirector.SymbolBorderWeight): > 1 draws the brigade DOUBLE
//    line (ink hugging both strip edges, center clipped), <= 1 draws a
//    single centered line covering that fraction of the strip (regiment
//    thin, battery baseline / park outline full).
// Both bands take a small constant clip-space offset toward the camera —
// belt-and-suspenders with the builder's geometric lift — and the border
// rides slightly prouder than the fill, so the frame wins the coplanar
// overlap deterministically instead of z-fighting the fill it sits on.
// NO _Time: nothing here animates, and if it ever does it rides
// _BattleWaveTime (determinism doctrine). Ships referenced from a
// Material ASSET (Assets/Battle/UnitSymbol.mat) — the magenta/stripping
// lesson.
//
// Two SubShaders, one per pipeline (Unity picks by RenderPipeline tag),
// the SoldierFigure/Flag house pattern: the HDRP ForwardOnly pass writes
// display-intent color straight into the pre-exposed buffer (no light
// loop, no exposure), the URP pass is the same math in half precision.
Shader "BattleAtlas/UnitSymbol"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _FillStyle("Fill Style", Float) = 0
        _BorderWeight("Border Weight", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "HDRenderPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _FillStyle;
                float _BorderWeight;
            CBUFFER_END

            // clip-space depth bias toward the camera, in NDC units: small
            // enough not to pop symbols through nearby ridges at grazing
            // angles, large enough to beat coplanar depth noise. The border
            // strips sit ON the fill grid at the same lift — their larger
            // bias resolves that overlap by construction.
            static const float FillDepthBias = 3.0e-5;
            static const float BorderDepthBias = 8.0e-5;
            // border band marker (SymbolMeshBuilder.BorderBandUvY): border
            // uv.y rides [2, 3]; anything past this threshold is border
            static const float BorderBandMin = 1.5;
            // border ink: the side hue darkened — the ring never vanishes,
            // even at the inactive fill desaturation
            static const float BorderDarken = 0.55;
            // brigade double line: the clipped center gap's half-width at
            // weight 1 (heavier weights fatten the two edge bands)
            static const float DoubleBorderGap = 0.7;
            // diagonal hatch: stripes per unit of uv.x, half ink half gap
            static const float HatchRepeat = 30.0;
            static const float HatchDuty = 0.5;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                // camera-relative world space (HDRP convention)
                float3 positionRWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionRWS);
                // constant NDC offset toward the camera, band-dependent
                float bias = input.uv.y >= BorderBandMin
                    ? BorderDepthBias : FillDepthBias;
                #if UNITY_REVERSED_Z
                output.positionCS.z += bias * output.positionCS.w;
                #else
                output.positionCS.z -= bias * output.positionCS.w;
                #endif
                output.uv = input.uv;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target0
            {
                if (input.uv.y >= BorderBandMin)
                {
                    // border band: t runs 0..1 across the strip, c runs
                    // 0 at the centerline to 1 at both edges — symmetric,
                    // so the four frame strips read alike whichever way
                    // their emit direction put the band's seams
                    float t = saturate(input.uv.y - 2.0);
                    float c = abs(t - 0.5) * 2.0;
                    if (_BorderWeight > 1.0)
                        clip(c - DoubleBorderGap / _BorderWeight);
                    else
                        clip(_BorderWeight - c);
                    return float4(_BaseColor.rgb * BorderDarken, 1.0);
                }
                // fill: provenance styling, UV-space, deterministic
                if (_FillStyle > 0.5)
                {
                    float hatch = frac(input.uv.x * HatchRepeat + input.uv.y);
                    if (_FillStyle > 1.5)
                    {
                        // reserved cross-hatch (contested — Phase 11 proper)
                        float crossHatch =
                            frac(input.uv.x * HatchRepeat - input.uv.y);
                        clip(max(HatchDuty - hatch, HatchDuty - crossHatch));
                    }
                    else
                    {
                        clip(HatchDuty - hatch); // gaps show the ground
                    }
                }
                return float4(_BaseColor.rgb, 1.0);
            }
            ENDHLSL
        }
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _FillStyle;
                half _BorderWeight;
            CBUFFER_END

            // same constants as the HDRP pass — see the header comments
            static const float FillDepthBias = 3.0e-5;
            static const float BorderDepthBias = 8.0e-5;
            static const half BorderBandMin = 1.5;
            static const half BorderDarken = 0.55;
            static const half DoubleBorderGap = 0.7;
            static const half HatchRepeat = 30.0;
            static const half HatchDuty = 0.5;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float bias = input.uv.y >= BorderBandMin
                    ? BorderDepthBias : FillDepthBias;
                #if UNITY_REVERSED_Z
                output.positionCS.z += bias * output.positionCS.w;
                #else
                output.positionCS.z -= bias * output.positionCS.w;
                #endif
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                if (input.uv.y >= BorderBandMin)
                {
                    half t = saturate(input.uv.y - 2.0);
                    half c = abs(t - 0.5) * 2.0;
                    if (_BorderWeight > 1.0)
                        clip(c - DoubleBorderGap / _BorderWeight);
                    else
                        clip(_BorderWeight - c);
                    return half4(_BaseColor.rgb * BorderDarken, 1.0);
                }
                if (_FillStyle > 0.5)
                {
                    half hatch = frac(input.uv.x * HatchRepeat + input.uv.y);
                    if (_FillStyle > 1.5)
                    {
                        half crossHatch =
                            frac(input.uv.x * HatchRepeat - input.uv.y);
                        clip(max(HatchDuty - hatch, HatchDuty - crossHatch));
                    }
                    else
                    {
                        clip(HatchDuty - hatch);
                    }
                }
                return half4(_BaseColor.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}
