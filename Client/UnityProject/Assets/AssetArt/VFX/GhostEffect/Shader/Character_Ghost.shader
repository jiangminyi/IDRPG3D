Shader "SGame/Character/Character_Ghost" 
{
	Properties
	{
		_MainTex ("Base Texture", 2D) = "white" {}
		_Color("Color", Color) = (0.5, 0.5, 0.5, 1.0)

		_MainTexStrength ("Main Tex Strength", Range(0, 1)) = 0
		_FresnelStrength ("Fresnel Strength", Range(0.01, 5)) = 0.5
		_FresnelColorStrength ("Fresnel Color Strength", Range(0, 5)) = 1
		_Alpha ("Alpha", Range(0, 5)) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent+10"
			"RenderType" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
		}

        Pass 
		{
			Name "DepthOnly"
			Tags { "LightMode" = "DepthOnly" }
			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#pragma vertex DepthVert
			#pragma fragment DepthFrag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings DepthVert(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				return output;
			}

			half4 DepthFrag(Varyings input) : SV_Target
			{
				return 0;
			}
			ENDHLSL
		}

        Pass 
        {
			Name "UniversalForward"
			Tags { "LightMode" = "UniversalForward" }
			Blend One OneMinusSrcAlpha			
			ZWrite Off

			HLSLPROGRAM
			#pragma vertex VertexGhost
			#pragma fragment FragGhost

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct a2v_ghost
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f_ghost
			{
				float4 positionCS : SV_POSITION;
				float2 uvTex0 : TEXCOORD0;
				half3 worldNormal : TEXCOORD1;
				half3 worldViewDir : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			half4 _Color;
			half _MainTexStrength;
			half _FresnelStrength;
			half _FresnelColorStrength;
			half _Alpha;
			CBUFFER_END

			v2f_ghost VertexGhost(a2v_ghost v)
			{
				v2f_ghost o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
				o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
				o.uvTex0 = TRANSFORM_TEX(v.texcoord, _MainTex);

				o.worldNormal = TransformObjectToWorldNormal(v.normalOS);
				o.worldViewDir = GetWorldSpaceViewDir(positionWS);

				return o;
			}

			half4 FragGhost(v2f_ghost i) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

				half4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvTex0);

				half3 worldNormal = normalize(i.worldNormal);
				half3 viewDirection = normalize(i.worldViewDir);

				half fresnelStrength = pow(1.002h - saturate(dot(worldNormal, viewDirection)), _FresnelStrength);
				half3 finalColor = _Color.rgb * fresnelStrength * _FresnelColorStrength;
				finalColor = lerp(finalColor, mainColor.rgb, saturate(_MainTexStrength));

				return half4(finalColor, saturate(fresnelStrength * _Alpha));
			}

			ENDHLSL
        }
	}
}
