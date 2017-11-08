// Upgrade NOTE: replaced 'unity_ObjectToWorld' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'unity_ObjectToWorld' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'unity_ObjectToWorld' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'unity_ObjectToWorld' with 'unity_ObjectToWorld'

Shader "SolidAR/ShadowComputeShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HoloDepthTextureL("HoloDepthMapL", 2D) = "white" {}
		_HoloDepthTextureR("HoloDepthMapR", 2D) = "white" {}
		_ProjShadowMap("ShadowMap", 2D) = "white" {}
	}
	SubShader
	{

		//Tags{ "RenderType" = "Opaque" "Queue" = "Transparent" }
		//Blend OneMinusDstColor One
		//ZTest Always
		//Cull Off
		//LOD 200

		//Tags{ "RenderType" = "Opaque" }
		Tags { "Queue" = "Overlay" "RenderType" = "Opaque" "LightMode" = "Always" /* Upgrade NOTE: changed from PixelOrNone to Always */ }
		//Tags{ "Queue" = "Geometry" }
		//Tags{ "Queue" = "Transparent" }
		//Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }
		//Tags{ "Queue" = "Overlay" "RenderType" = "Opaque" }
		//Cull Off 
		//ZWrite Off 
		//ZTest Always
		//LOD 100
		//Cull Off ZWrite Off ZTest Always

		//Cull Off ZWrite On

		//UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				//UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 position : TEXCOORD1;
				float4 lightViewPosition : TEXCOORD2;
				float3 worldPos : COLOR0;
			};

			int _enableARShadows = 1;
			int _enableOcclusionShadows = 1;

			sampler2D _MainTex;
			//float4 _MainTex_ST;
			//sampler2D_float _CameraDepthTexture; //the depth texture
			//sampler2D_float _LastCameraDepthTexture;
			sampler2D_float _HoloDepthTextureL;
			sampler2D_float _HoloDepthTextureR;
			sampler2D_float _ProjShadowMap;

			float4x4 _holoPL;
			float4x4 _holoVL;
			float4x4 _holoML;
			float4x4 _holoPR;
			float4x4 _holoVR;
			float4x4 _holoMR;
			float4x4 _projP;
			float4x4 _projV;
			float4x4 _projM;
			
			int force_eye = 0;

			float4x4 biasMatrix = float4x4(0.5, 0.0, 0.0, 0.0,
				0.0, 0.5, 0.0, 0.0,
				0.0, 0.0, 0.5, 0.0,
				0.5, 0.5, 0.5, 1.0);

			float4x4 DepthBiasMVP;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				
				int eye; //0;left, 1:right
				eye = unity_StereoEyeIndex;
				if (force_eye != 0)
					eye = force_eye - 1;
				//eye = 1;

				float4x4 _holoP = _holoPL;
				float4x4 _holoV = _holoVL;
				float4x4 _holoM = _holoML;
				if (eye == 1) {
					_holoP = _holoPR;
					_holoV = _holoVR;
					_holoM = _holoMR;
				}

				o.position = mul(_holoP, mul(_holoV, mul(unity_ObjectToWorld,v.vertex)));

				o.lightViewPosition = mul(_projP, mul(_projV, mul(unity_ObjectToWorld, v.vertex)));
				
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				int eye; //0;left, 1:right
				eye = unity_StereoEyeIndex;
				if (force_eye != 0)
					eye = force_eye - 1;
				//eye = 1;

	
				float shadowCoeff = 1.0f;
				
				//Adding Occlusion Shadows
				if (_enableOcclusionShadows && shadowCoeff > 0) 
				{
					// Compute pixel depth for Occlusion shadowing
					float2 projectTexCoord;
					float depth = (i.position.z / i.position.w);
					projectTexCoord.x = i.position.x / i.position.w / 2.0f + 0.5f;
					projectTexCoord.y = -i.position.y / i.position.w / 2.0f + 0.5f;
					
					float holoDepth = 0.0f;
					if (eye == 0) {
#if UNITY_VERSION >= 550
						holoDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_HoloDepthTextureL, projectTexCoord))); //UNITY 5.5
#else
						holoDepth = (UNITY_SAMPLE_DEPTH(tex2D(_HoloDepthTextureL, projectTexCoord)));
#endif
					}
					else
					{
#if UNITY_VERSION >= 550
						holoDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_HoloDepthTextureR, projectTexCoord))); //UNITY 5.5
#else
						holoDepth = (UNITY_SAMPLE_DEPTH(tex2D(_HoloDepthTextureR, projectTexCoord)));
#endif
					}

					if (saturate(projectTexCoord.x) == projectTexCoord.x && saturate(projectTexCoord.y) == projectTexCoord.y)
						shadowCoeff = holoDepth < depth ? 0.0f : 1.0f;
				}

				//ADDING normal shadows
				if (_enableARShadows && 
					shadowCoeff > 0) {
					// Compute pixel depth for AR shadowing
					float2 projectTexCoord;
					float depth = i.lightViewPosition.z / i.lightViewPosition.w;
					projectTexCoord.x = i.lightViewPosition.x / i.lightViewPosition.w / 2.0f + 0.5f;
					projectTexCoord.y = -i.lightViewPosition.y / i.lightViewPosition.w / 2.0f + 0.5f;
#if UNITY_VERSION >= 550
					float shadowDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_ProjShadowMap, projectTexCoord))); //UNITY 5.5
#else
					float shadowDepth = (UNITY_SAMPLE_DEPTH(tex2D(_ProjShadowMap, projectTexCoord)));
#endif
					if (saturate(projectTexCoord.x) == projectTexCoord.x && saturate(projectTexCoord.y) == projectTexCoord.y)
						shadowCoeff = shadowDepth < depth ? 0.0f : 1.0f;
				}

				return fixed4(shadowCoeff, shadowCoeff, shadowCoeff, 1.0f);
			}
			ENDCG
		}
	}
}
