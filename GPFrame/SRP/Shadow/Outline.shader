Shader "Custom/Outline"
{
	Properties
	{
		_Outline("Thick of Outline",range(0,0.1)) = 0.03
        _Factor("Factor",range(0,1)) = 0.5
        _OutColor("OutColor",color) = (0,0,0,0)
	}
	
	SubShader
	{
		Tags {"RenderPipeline" = "LightweightPipeline" "RenderType" = "Opaque" }
		Pass//处理光照前的pass渲染
        {
			Name "OUTLINE"
			Tags { "LightMode" = "OUTLINE" }
            Cull Front
			//Cull [_Cull]
            ZWrite On
			//Offset 1 , 0
            //Lighting On
            HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			
            #pragma vertex outlineVert
            #pragma fragment outlineFrag
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            float _Outline;
            float _Factor;
            float4 _OutColor;
			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS         : NORMAL;
			};
            struct v2f {
                float4 pos:SV_POSITION;
                //UNITY_FOG_COORDS(0)
            };

            v2f outlineVert(Attributes v) {
                v2f o;
                float3 dir = normalize(v.positionOS.xyz);
                float3 dir2 = v.normalOS;
                float D = dot(dir,dir2);
                dir = dir * sign(D);
                dir = dir * _Factor + dir2 * (1 - _Factor);
                v.positionOS.xyz += dir * _Outline;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                o.pos = vertexInput.positionCS;//UnityObjectToClipPos(v.vertex);
                //UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            half4 outlineFrag(v2f i) :COLOR
            {
                half4 c = _OutColor;
                //UNITY_APPLY_FOG(i.fogCoord, c);
                return c;
            }
            ENDHLSL
        }

	}
}
