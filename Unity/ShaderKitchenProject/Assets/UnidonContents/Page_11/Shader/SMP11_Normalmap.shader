// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "ShaderDic/SMP11_NormalMap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BumpMap ("Normal", 2D) = "bump" {}
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
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 TangentW : TEXCOORD2;
				float3 NormalW : TEXCOORD3;
				float3 BinormalW : TEXCOORD4;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;
			
			float4 _LightColor0;// color of light source (from "Lighting.cginc")
			
			v2f vert (appdata v)
			{
				v2f o;
				
				//tangent world
				float4x4 modelMatrix = unity_ObjectToWorld;
				float4x4 modelMatrixInverse = unity_WorldToObject;
				
				o.TangentW = normalize(mul(modelMatrix,float4(v.tangent.xyz,0.0)).xyz);
				o.NormalW = normalize(mul(float4(v.normal,0.0),modelMatrixInverse).xyz);
				o.BinormalW = normalize(cross(o.NormalW,o.TangentW) * v.tangent.w);
				
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 encodedNormal = tex2D(_BumpMap,_BumpMap_ST.xy * i.uv.xy + _BumpMap_ST.zw);
				float3 localCoords = float3(2.0 * encodedNormal.a - 1.0,2.0 * encodedNormal.g - 1.0,1.0);
				
				float3x3 local2WorldTranspose = float3x3(i.TangentW,i.BinormalW,i.NormalW);

				float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
				float3 ViewDirection = normalize( _WorldSpaceCameraPos - i.vertex.xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0);
				float3 HalfLambert = saturate(dot(lightDirection,normalDirection)) * 0.5 + 0.5;//Half-Lambert
				float3 lambertColor = _LightColor0 * HalfLambert;
			
				fixed4 col = tex2D(_MainTex, i.uv) * float4(lambertColor,1.0);
			
				return col;
			}
			ENDCG
		}
	}
}
