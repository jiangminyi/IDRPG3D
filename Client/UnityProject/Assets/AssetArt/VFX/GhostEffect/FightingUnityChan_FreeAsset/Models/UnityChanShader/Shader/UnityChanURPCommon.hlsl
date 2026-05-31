#ifndef UNITYCHAN_URP_COMMON_INCLUDED
#define UNITYCHAN_URP_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _Color;
float4 _ShadowColor;
float4 _MainTex_ST;
float _SpecularPower;
float _EdgeThickness;
float _Cutoff;
CBUFFER_END

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

struct UnityChanAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct UnityChanVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    half3 normalWS : TEXCOORD1;
    half3 viewDirWS : TEXCOORD2;
    float3 positionWS : TEXCOORD3;
    UNITY_VERTEX_OUTPUT_STEREO
};

UnityChanVaryings UnityChanForwardVertex(UnityChanAttributes input)
{
    UnityChanVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    output.positionCS = positionInputs.positionCS;
    output.positionWS = positionInputs.positionWS;
    output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
    output.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
    output.uv = TRANSFORM_TEX(input.uv, _MainTex);
    return output;
}

half4 UnityChanForwardFragment(UnityChanVaryings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half4 baseSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
#if defined(UNITYCHAN_ALPHA_CLIP)
    clip(baseSample.a - max(_Cutoff, 0.1));
#endif

    half3 normalWS = NormalizeNormalPerPixel(input.normalWS);
    half3 viewDirWS = SafeNormalize(input.viewDirWS);
    Light mainLight = GetMainLight();

    half ndotl = saturate(dot(normalWS, mainLight.direction));
    half toonRamp = smoothstep(0.25h, 0.75h, ndotl);
    half3 ambient = SampleSH(normalWS);
    half3 shadowColor = baseSample.rgb * _ShadowColor.rgb;
    half3 litColor = baseSample.rgb * (ambient + mainLight.color * toonRamp);
    half3 finalColor = lerp(shadowColor, litColor, toonRamp);

    half rim = pow(saturate(1.0h - dot(normalWS, viewDirWS)), 3.0h);
    finalColor += rim * baseSample.rgb * 0.25h;

#if defined(UNITYCHAN_SPECULAR)
    half3 halfDir = SafeNormalize(mainLight.direction + viewDirWS);
    half specularPower = max((half)_SpecularPower, 8.0h);
    half specular = pow(saturate(dot(normalWS, halfDir)), specularPower);
    finalColor += specular * mainLight.color * 0.25h;
#endif

    return half4(finalColor, baseSample.a);
}

struct UnityChanOutlineVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};

UnityChanOutlineVaryings UnityChanOutlineVertex(UnityChanAttributes input)
{
    UnityChanOutlineVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    half3 normalWS = TransformObjectToWorldNormal(input.normalOS);
    positionWS += normalWS * max(_EdgeThickness, 0.0) * 0.003;

    output.positionCS = TransformWorldToHClip(positionWS);
    output.uv = TRANSFORM_TEX(input.uv, _MainTex);
    return output;
}

half4 UnityChanOutlineFragment(UnityChanOutlineVaryings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    half4 baseSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
#if defined(UNITYCHAN_ALPHA_CLIP)
    clip(baseSample.a - max(_Cutoff, 0.1));
#endif
    return half4(baseSample.rgb * 0.25h, baseSample.a);
}

#endif
