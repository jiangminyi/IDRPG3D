Shader "UnityChan/Hair"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0.8, 0.8, 1, 1)
        _SpecularPower ("Specular Power", Float) = 20
        _EdgeThickness ("Outline Thickness", Float) = 1
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

        _MainTex ("Diffuse", 2D) = "white" {}
        _FalloffSampler ("Falloff Control", 2D) = "white" {}
        _RimLightSampler ("RimLight Control", 2D) = "white" {}
        _SpecularReflectionSampler ("Specular / Reflection Mask", 2D) = "white" {}
        _EnvMapSampler ("Environment Map", 2D) = "white" {}
        _NormalMapSampler ("Normal Map", 2D) = "bump" {}
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZTest LEqual

            HLSLPROGRAM
            #define UNITYCHAN_SPECULAR 1
            #pragma vertex UnityChanForwardVertex
            #pragma fragment UnityChanForwardFragment
            #include "UnityChanURPCommon.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZTest Less

            HLSLPROGRAM
            #pragma vertex UnityChanOutlineVertex
            #pragma fragment UnityChanOutlineFragment
            #include "UnityChanURPCommon.hlsl"
            ENDHLSL
        }
    }
}
