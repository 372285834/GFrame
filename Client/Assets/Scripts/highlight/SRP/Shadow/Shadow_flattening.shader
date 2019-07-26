Shader "Custom/ShadowFlattening"
{
	Properties
	{
		_SColor("_SColor" , color) = (0,0,0,0.5)
		//_MainTex ("Texture", 2D) = "white" {}
		_High("High" , float) = 0
		_ShadowFalloff("ShadowFalloff" , float) = 0
		_LightDir ("LightDir", vector) = (1,1,1,0)
	}
	
	SubShader
	{
		Tags { "RenderPipeline" = "LightweightPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 700
		Pass
		{
			Name "PlanarShadow"
			Tags{"LightMode" = "PlanarShadow"}
			Stencil
			{
				Ref 0
				Comp equal
				Pass incrWrap
				Fail keep
				ZFail keep
			}
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			//深度稍微偏移防止阴影与地面穿插
			Offset -1 , 0
			Cull Front
			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			
            #pragma vertex vert
            #pragma fragment frag
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			
			struct appdata
			{
				float4 positionOS : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};
			
			float4 _LightDir;
			half4 _SColor;
			float _High;
			float _ShadowFalloff;
			float3 ShadowProjectPos(float4 vertPos)
			{
				float3 shadowPos;
				
				//得到顶点的世界空间坐标
				float3 worldPos = mul(unity_ObjectToWorld , vertPos).xyz;

				//灯光方向
				float3 lightDir = normalize(_LightDir.xyz);

				//计算阴影的世界空间坐标(如果顶点低于地面，则阴影点实际就是顶点在世界空间的位置，不做改变)
				shadowPos.y = min(worldPos.y , _LightDir.w);
				shadowPos.xz = worldPos.xz - lightDir.xz * max(0 , worldPos.y - _LightDir.w) / lightDir.y; 
				//shadowPos.xz=worldPos.xz-lightDir.xz*max(0,worldPos.y-_LightDir.w)/(lightDir.y-_LightDir.w);
				
				return shadowPos;
			}
			v2f vert (appdata v)
			{
				v2f o;
				//得到阴影的世界空间坐标
				//VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
				float3 shadowPos = ShadowProjectPos(v.positionOS);
				shadowPos.y += _High;
				//_LightDir.w += _High;
				//转换到裁切空间
				o.vertex = TransformWorldToHClip(shadowPos);//UnityWorldToClipPos(shadowPos);

				//得到中心点世界坐标
				float3 center =float3(unity_ObjectToWorld[0].w , _LightDir.w , unity_ObjectToWorld[2].w);
				//center.y += _High;
				//计算阴影衰减
				float falloff = 1-saturate(distance(shadowPos , center) * _ShadowFalloff);

				//阴影颜色
				o.color = _SColor; 
				o.color.a *= falloff;
		
				//float4 wPos = mul(unity_ObjectToWorld , v.positionOS);
				//wPos.y = _High;
				//float4 lPos = mul(unity_WorldToObject , wPos);
				
				//o.vertex =UnityObjectToClipPos(lPos);
				//o.color = _Color; 
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{

				return i.color;
			}
			ENDHLSL
		}
	}
}
