Shader "ShaderDic/SMP5_Rim"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("AmbientColor",Color) = (1,1,1,1)
		_RimColor ("Rim Color",Color) = (1,1,1,1)
		_RimPower("Rim Power",Range(0.5,8.0)) = 3.0
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
				float4 EyeVector : COLOR1;		//semantic
				float4 ambientColor : COLOR2;	//semantic
				float4 RimColor : COLOR3;		//semantic
			};

			sampler2D _MainTex;
			float4 _Color;
			float4 _MainTex_ST;
			float4 _LightColor0;
			float4 _RimColor;
			float _RimPower;
			
			v2f vert (appdata v)
			{
				v2f o;

				o.normal = normalize(mul(v.normal,_World2Object));//Get Vector Face Normal > _World2Object 現在のモデル行列の逆行列
				o.color = normalize(_WorldSpaceLightPos0); //Get Vector Light > _WorldSpaceLightPos0 UnityのScene上での一つ目のLightのベクトル情報（Posだけど位置ではじゃないです）

				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.EyeVector = normalize(float4(_WorldSpaceCameraPos,1.0) - o.vertex); //視点の所得
						
				o.ambientColor = float4(UNITY_LIGHTMODEL_AMBIENT.rgb * _Color,1.0);

				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
				float RimValue = 1.0 - saturate(dot(v.normal , viewDir)); //Rimの具合を決めます
				o.RimColor = smoothstep(1.0 - _RimPower, 1.0 ,RimValue) * _RimColor;
				//o.RimColor = pow(RimValue,_RimPower) * _RimColor;
				
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex,i.uv);
						
				float3 normalDirection = i.normal;
				float3 lightDirection = i.color;
				float3 ViewDirection = i.EyeVector;
				float4 ambientColor = i.ambientColor;
				float4 NL = saturate(dot(normalDirection,lightDirection)) + ambientColor;
				
				float3 Reflect = normalize(2 * NL * normalDirection - lightDirection);
				float4 SpecularLight = 2 * pow(saturate(dot(Reflect,ViewDirection)),8); // pow(saturate(dot(Reflect,ViewDirection))　, param)  > paramでハイライトの絞りの大きさが変わります 

				float4 Rim = i.RimColor;			
									
				col = (col * NL + SpecularLight) + Rim;
							
				return col;
			}
			ENDCG
		}
	}
}
