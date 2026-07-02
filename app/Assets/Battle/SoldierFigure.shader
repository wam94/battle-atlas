// Two-tone soldier figures: multiplies the mesh's Color32 channel
// (trousers/coat/flesh bands, see InstancedMeshes.BuildSoldier*) by the
// per-side _BaseColor from the MaterialPropertyBlock, with a simple
// half-lambert against the main light — the cheapest possible lit shader
// for 400-figure instanced batches. Ships referenced from a Material
// ASSET (Assets/Battle/SoldierFigure.mat): runtime-created materials
// render magenta in device builds because their shader gets stripped.
Shader "BattleAtlas/SoldierVertexTint"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
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
