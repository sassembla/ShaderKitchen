// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "ShaderDic/SMP4_Ambient"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("AmbientColor",Color) = (1,1,1,1)
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
				float4 normal : NORMAL;
				float4 EyeVector : COLOR1;//semantic
				float4 ambientColor : COLOR2;//semantic
			};

			sampler2D _MainTex;
			float4 _Color;
			float4 _MainTex_ST;
			float4 _LightColor0;
			
			v2f vert (appdata v)
			{
				v2f o;

				o.normal = normalize(mul(v.normal,unity_WorldToObject));//Get Vector Face Normal > _World2Object 現在のモデル行列の逆行列
				o.color = normalize(_WorldSpaceLightPos0); //Get Vector Light > _WorldSpaceLightPos0 UnityのScene上での一つ目のLightのベクトル情報（Posだけど位置ではじゃないです）

				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.EyeVector = normalize(float4(_WorldSpaceCameraPos,1.0) - o.vertex); //視点の所得
				
						
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.ambientColor = float4(UNITY_LIGHTMODEL_AMBIENT.rgb * _Color,1.0);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 normalDirection = i.normal;
				float3 lightDirection = i.color;
				float3 ViewDirection = i.EyeVector;
				float4 ambientColor = i.ambientColor;
				float4 NL = saturate(dot(normalDirection,lightDirection)) + ambientColor;
				
				float3 Reflect = normalize(2 * NL * normalDirection - lightDirection);
				float4 SpecularLight = 2 * pow(saturate(dot(Reflect,ViewDirection)),8); // pow(saturate(dot(Reflect,ViewDirection))　, param)  > paramでハイライトの絞りの大きさが変わります 
			
				fixed4 col = tex2D(_MainTex, i.uv) * NL + SpecularLight;
							
				return col;
			}
			ENDCG
		}
	}
}
