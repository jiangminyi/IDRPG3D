Shader "UnityChan/Skin - Transparent"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0.8, 0.8, 1, 1)
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5

        _MainTex ("Diffuse", 2D) = "white" {}
        _FalloffSampler ("Falloff Control", 2D) = "white" {}
        _RimLightSampler ("RimLight Control", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex UnityChanForwardVertex
            #pragma fragment UnityChanForwardFragment
            #include "UnityChanURPCommon.hlsl"
            ENDHLSL
        }
    }
}
