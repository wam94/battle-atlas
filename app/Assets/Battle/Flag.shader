// Instanced unit flags with a vertex wave. The wave runs on _BattleWaveTime
// — a global BattleDirector sets from the battle clock every frame, NOT
// _Time — so scrubbing the timeline replays the identical cloth motion
// (determinism even in presentation). Phase comes from the instance's world
// position: every flag desyncs from its neighbors with zero CPU work.
// Amplitude is pinned to zero at the staff edge (x=0) and grows toward the
// fly, per the classic flag-wave construction. Ships referenced from a
// Material ASSET (Assets/Battle/Flag.mat) — the magenta/stripping lesson.
//
// Two SubShaders, one per pipeline (Unity picks by RenderPipeline tag).
// The HDRP pass keeps the identical wave math; its phase uses the flag's
// ABSOLUTE world origin (GetObjectAbsolutePositionWS) because HDRP renders
// camera-relative — the raw UNITY_MATRIX_M translation would change with
// the camera and desync the cloth from the deterministic battle record.
// Shading: analytic wave shade x _BaseColor, written straight into HDRP's
// pre-exposed buffer (no light loop, no exposure — see SoldierFigure).
Shader "BattleAtlas/Flag"
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
            // the 45-vert quad is single-sided geometry; both faces draw
            Cull Off
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

            // battle-clock seconds (Shader.SetGlobalFloat in BattleDirector)
            float _BattleWaveTime;

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float shade : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                // per-flag phase from the instance's ABSOLUTE world
                // translation (camera-relative M would break determinism)
                float3 origin = GetObjectAbsolutePositionWS();
                float phase = (origin.x + origin.z) * 0.35;
                // pinned at the staff (x=0), full amplitude at the 1.8m fly
                float pin = saturate(input.positionOS.x / 1.8);
                float wave = sin(_BattleWaveTime * 6.0 + phase + input.positionOS.x * 3.5);
                float4 positionOS = input.positionOS;
                positionOS.z += wave * 0.18 * pin;
                float3 positionRWS = TransformObjectToWorld(positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionRWS);
                // cheap cloth read: troughs darken slightly with the wave
                output.shade = 0.85 + 0.15 * wave;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target0
            {
                // full side color x analytic shade, exactly the URP pass:
                // the flag is the unit's identity and never desaturates
                return float4(_BaseColor.rgb * input.shade, 1.0);
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
            // the 45-vert quad is single-sided geometry; both faces draw
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END

            // battle-clock seconds (Shader.SetGlobalFloat in BattleDirector)
            float _BattleWaveTime;

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half shade : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                // per-flag phase from the instance's world translation
                float3 origin = float3(
                    UNITY_MATRIX_M._m03, UNITY_MATRIX_M._m13, UNITY_MATRIX_M._m23);
                float phase = (origin.x + origin.z) * 0.35;
                // pinned at the staff (x=0), full amplitude at the 1.8m fly
                float pin = saturate(input.positionOS.x / 1.8);
                float wave = sin(_BattleWaveTime * 6.0 + phase + input.positionOS.x * 3.5);
                float4 positionOS = input.positionOS;
                positionOS.z += wave * 0.18 * pin;
                output.positionCS = TransformObjectToHClip(positionOS.xyz);
                // cheap cloth read: troughs darken slightly with the wave
                output.shade = 0.85 + 0.15 * wave;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(_BaseColor.rgb * input.shade, 1.0);
            }
            ENDHLSL
        }
    }
}
