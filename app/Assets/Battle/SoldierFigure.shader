// Two-tone soldier figures: multiplies the mesh's Color32 channel
// (trousers/coat/flesh bands, see InstancedMeshes.BuildSoldier*) by the
// per-side _BaseColor from the MaterialPropertyBlock, with a simple
// half-lambert against the sun — the cheapest possible lit shader
// for 400-figure instanced batches. Ships referenced from a Material
// ASSET (Assets/Battle/SoldierFigure.mat): runtime-created materials
// render magenta in device builds because their shader gets stripped.
//
// Two SubShaders, one per pipeline (Unity picks by RenderPipeline tag):
//  - HDRP (Phase 4, the project's active pipeline): a self-contained
//    ForwardOnly pass. It does NOT consult HDRP's light loop or physical
//    exposure — the sun arrives as the deterministic globals
//    _BattleSunDirWS/_BattleSunColor that SunDirector computes from the
//    battle clock (normalized URP-scale intensity, not lux), and the
//    fragment writes straight into HDRP's pre-exposed color buffer, so
//    the figures keep the Atlas's authored legibility regardless of the
//    volume's EV. Deterministic: no _Time, no light-loop state.
//  - URP (kept beside the preserved URP settings history): the original
//    GetMainLight() path, unchanged.
Shader "BattleAtlas/SoldierVertexTint"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
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
            CBUFFER_END

            // deterministic sun globals — SunDirector sets these from the
            // battle clock every frame (xyz = direction TOWARD the sun;
            // color x normalized intensity). NOT HDRP physical units: the
            // pass writes display-intent values into the pre-exposed buffer.
            float4 _BattleSunDirWS;
            float4 _BattleSunColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR0;
                float3 normalWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                // camera-relative world space (HDRP convention); the clip
                // transform matches, so no absolute positions needed here
                float3 positionRWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionRWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.color = input.color * _BaseColor;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target0
            {
                float ndl = saturate(dot(normalize(input.normalWS), _BattleSunDirWS.xyz));
                // same half-lambert floor as the URP pass: shaded sides stay
                // readable, matching the flat sand-table look of the scene
                float3 lit = input.color.rgb * _BattleSunColor.rgb * (0.35 + 0.65 * ndl);
                return float4(lit, 1.0);
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR0;
                float3 normalWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.color = input.color * _BaseColor;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                half ndl = saturate(dot(normalize(input.normalWS), mainLight.direction));
                // half-lambert floor keeps shaded sides readable, matching
                // the flat sand-table look of the rest of the scene
                half3 lit = input.color.rgb * mainLight.color.rgb * (0.35 + 0.65 * ndl);
                return half4(lit, 1.0);
            }
            ENDHLSL
        }
    }
}
