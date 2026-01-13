Shader "AlchemicalArts/Temperature/TemperatureDisplayer" {
	Properties {
		
	}
	SubShader {

		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass {

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.5

			#include "UnityCG.cginc"
			
			StructuredBuffer<float2> Positions;
			StructuredBuffer<float> Temperatures;
			float Radius;
			float MaxTemperature;
			Texture2D<float4> ColourMap;

			SamplerState linear_clamp_sampler;

			struct FluidData
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 colour : TEXCOORD1;
			};

			FluidData vert (appdata_full v, uint instanceID : SV_InstanceID)
			{
				float speed = Temperatures[instanceID];
				float speedT = saturate(speed / MaxTemperature);
				
				float3 centreWorld = float3(Positions[instanceID], 0);
				float3 worldVertPos = centreWorld + mul(unity_ObjectToWorld, v.vertex * Radius);
				float3 objectVertPos = mul(unity_WorldToObject, float4(worldVertPos.xyz, 1));

				FluidData o;
				o.uv = v.texcoord;
				o.position = UnityObjectToClipPos(objectVertPos);
				o.colour = ColourMap.SampleLevel(linear_clamp_sampler, float2(speedT, 0.5), 0);

				return o;
			}


			float4 frag (FluidData i) : SV_Target
			{
				float2 centreOffset = (i.uv.xy - 0.5) * 2;
				float sqrDst = dot(centreOffset, centreOffset);
				float delta = fwidth(sqrt(sqrDst));
				float alpha = 1 - smoothstep(1 - delta, 1 + delta, sqrDst);

				float3 colour = i.colour;
				return float4(colour, alpha);
			}

			ENDCG
		}
	}
}