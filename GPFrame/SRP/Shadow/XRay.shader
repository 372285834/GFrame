Shader "Custom/XRay"
{
	Properties
	{
        _XRayColor("XRayColor",color) = (1,1,1,1)
		_NoiseTex ("NoiseTex (R)",2D) = "white"{}
		_DissolveFactor ("DissolveFactor",Float) = 0.5
	}
	
	SubShader
	{
		Tags {"Queue" = "Transparent" "RenderPipeline" = "LightweightPipeline" "RenderType" = "Transparent" }
		Pass
		{
			Name "XRAY"
			Tags { "LightMode" = "XRAY" }
			
			Blend SrcAlpha One
			ZWrite Off
			ZTest Greater
			//Cull Back
			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
			half4 _XRayColor;
			float _DissolveFactor;
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS         : NORMAL;
				float2 uv : TEXCOORD0;
			};
			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 normal : normal;
				float2 uv : TEXCOORD0;
				float3 viewDir : TEXCOORD1;
			};
 
			v2f vert (Attributes v)
			{
				v2f o;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS);
                o.pos = vertexInput.positionCS;
				o.viewDir = vertexInput.positionVS;
				o.normal = v.normalOS;
				o.uv = v.uv;
				return o;
			}
 
			half4 frag(v2f i) : SV_Target
			{
				float3 normal = normalize(i.normal);
				float3 viewDir = normalize(i.viewDir);
				float rim = 1 - dot(normal, viewDir);
				float noiseValue = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv).r;
                if(noiseValue <= _DissolveFactor)
                {
                    discard;
                }
				return _XRayColor * rim;
			}
			
			#pragma vertex vert
			#pragma fragment frag
			ENDHLSL
		}

	}
}
