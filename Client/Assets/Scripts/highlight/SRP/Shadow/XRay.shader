Shader "Custom/XRay"
{
	Properties
	{
		//_MainTex ("Texture", 2D) = "white" {}
        _XRayColor("XRayColor",color) = (1,1,1,1)
		//_Factor ("Factor",Float) = 0.5
		//_NoiseTex ("NoiseTex (R)",2D) = "white"{}
		//_DissolveFactor ("DissolveFactor",Float) = 0.5
	}
	
	SubShader
	{
		Tags {"Queue" = "Transparent" "RenderPipeline" = "LightweightPipeline" "RenderType" = "Transparent" }
		Pass
		{
			Name "XRAY"
			Tags { "LightMode" = "XRAY" }
			//ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest Greater
			Cull Back
			
		Stencil
		{
			Ref 2
			Comp equal
			Pass incrWrap
			Fail zero
			ZFail zero
		}
			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			//TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			//TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
			half4 _XRayColor;
			//float _DissolveFactor;
			//float _Factor;
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS         : NORMAL;
				float4 tangentOS        : TANGENT;
				float2 uv : TEXCOORD0;
			};
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float ndv :		TEXCOORD1;
				//float3 normal : TEXCOORD2;
				//float3 viewDir : TEXCOORD3;
			};
 
			v2f vert (Attributes v)
			{
				v2f o;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS);
                o.pos = vertexInput.positionCS;
				VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangentOS);
				half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
				//o.viewDir = normalize(viewDirWS);//vertexInput.positionVS;
				//o.normal = normalInput.normalWS;//v.normalOS;
				o.uv = v.uv;
				float NdotV = saturate(dot(normalInput.normalWS,normalize(viewDirWS)));
				o.ndv = NdotV;
				return o;
			}
 
			half4 frag(v2f i) : SV_Target
			{
				//float3 normal = normalize(i.normal);
				//float3 viewDir = normalize(i.viewDir);
				float rim = 1 - i.ndv;
				//float4 co = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				//float noiseValue = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv).r;
               // if(noiseValue <= _DissolveFactor)
               // {
               //     discard;
                //}
				return _XRayColor * rim;
			}
			
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}

	}
}
