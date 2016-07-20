// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "ShaderDic/SMP6_HalfLambert"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"} // LightModeを設定
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _LightColor0;
			
			v2f vert (appdata v)
			{
				v2f o;
				
				float3 normalDirection = normalize(mul(v.normal,unity_WorldToObject));
				float3 lightDirection = normalize(_WorldSpaceLightPos0);
		
				float3 HalfLambert = saturate(dot(lightDirection,normalDirection)) * 0.5 + 0.5;//Half-Lambert
				float3 lambertColor = 1.0 * _LightColor0 * HalfLambert;
				
				o.color = float4(lambertColor,1.0);
				
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
							
				return col;
			}
			ENDCG
		}
	}
}
