Shader "SolidAR/RenderDepth"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		
		Tags{ "RenderType" = "Opaque" }

		//UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

		Pass
		{
			Cull Off ZWrite Off //ZTest Always
			Lighting Off

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
		
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 projPos : TEXCOORD1; //Screen position of pos
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.vertex.x+=01.1f;
				o.uv = v.uv;
				o.projPos = ComputeScreenPos(o.vertex);
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D_float _CameraDepthTexture; //the depth texture
			sampler2D_float _LastCameraDepthTexture;

			fixed4 frag (v2f i) : SV_Target
			{
			fixed4 col = tex2D(_MainTex, i.uv);
			//fixed4 col = fixed4(1,1,1,1);
			float4 projCoords = UNITY_PROJ_COORD(i.projPos);
			//float depth = Linear01Depth(tex2Dproj(_CameraDepthTexture, projCoords)).r;
			//float depth = tex2Dproj(_CameraDepthTexture, projCoords);
			//float depth = tex2D(_LastCameraDepthTexture, i.uv) < 1.0f ? 0.0f : 1.0f;
			//float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
			//float depth = (tex2D(_LastCameraDepthTexture, i.uv));
			
			float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_LastCameraDepthTexture, i.uv)));  //OK
			//float _NearClip = 0.01f;
			//float _FarClip = 20;
			//// Compute pixel depth for shadowing
			//float depth = i.position.z / i.position.w;
			//// Now linearise using a formula by Humus, drawn from the near and far clipping planes of the camera.
			//float sceneDepth = _NearClip * (depth + 1.0) / (_FarClip + _NearClip - depth * (_FarClip - _NearClip));


			//depth = (UNITY_SAMPLE_DEPTH(tex2D(_LastCameraDepthTexture, i.uv)));

			//float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
			////depth = LinearEyeDepth(depth);


			//float depth = tex2D(_LastCameraDepthTexture, i.uv)*1000;
			//col.rgb *= depth;
			//col.r = depth;
			//col.g = depth;
			//col.b = depth;
			// just invert the colors
			//col = 1 - col;
			//return fixed4(1, 1, 1, 1);
			//return fixed4(i.depth.x, i.depth.x, i.depth.x, 1);
			return fixed4(depth, depth, depth, 1);
			//return col;
			}
			ENDCG
		}
	}
	//FallBack Off
	FallBack "Diffuse"
}
