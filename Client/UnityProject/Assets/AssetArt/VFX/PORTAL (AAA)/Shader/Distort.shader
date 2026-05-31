Shader "Custom/Distortion"
{
    Properties
    {
        _Opacity("Opacity", Range(0.0, 2.0)) = 0.5
        _Intensity("Intensity", Range(0.0, 10.0)) = 1.0
        _Distortion("Distortion", Range(0.0, 2.0)) = 0.05
        _MainTex("Particle Texture", 2D) = "white" {}
        _DistTex("Distortion Texture", 2D) = "white" {}
        _InvFade("Soft Particles Factor", Range(0.01, 8.0)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile_particles

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DistTex);
            SAMPLER(sampler_DistTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _DistTex_ST;
                half _Opacity;
                half _Intensity;
                half _Distortion;
                half _InvFade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                float4 positionNDC : TEXCOORD2;
                float2 uvDist : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionNDC = positionInputs.positionNDC;
                output.color = input.color * _Intensity;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.uvDist = TRANSFORM_TEX(input.uv, _DistTex);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half SoftParticleAlpha(float4 positionNDC)
            {
                #if defined(SOFTPARTICLES_ON) || defined(_SOFTPARTICLES_ON)
                    float2 screenUV = positionNDC.xy / positionNDC.w;
                    float rawDepth = SampleSceneDepth(screenUV);
                    float sceneZ = (unity_OrthoParams.w == 0.0) ? LinearEyeDepth(rawDepth, _ZBufferParams) : LinearDepthToEyeDepth(rawDepth);
                    float particleZ = LinearEyeDepth(positionNDC.z / positionNDC.w, _ZBufferParams);
                    return saturate(_InvFade * (sceneZ - particleZ));
                #else
                    return 1.0h;
                #endif
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half mainAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                half4 distortionSample = SAMPLE_TEXTURE2D(_DistTex, sampler_DistTex, input.uvDist);
                half2 distortion = UnpackNormal(distortionSample).rg;

                float4 screenPos = input.positionNDC;
                screenPos.xy += distortion * _Distortion;

                half4 color = half4(SampleSceneColor(screenPos.xy / screenPos.w), input.color.a * mainAlpha * SoftParticleAlpha(input.positionNDC) * _Opacity);
                color.rgb = MixFog(color.rgb, input.fogCoord);
                return color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
