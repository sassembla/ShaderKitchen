// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "ShaderDic/SMP14_Toon"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Texture", 2D) = "white" {}
		_RampTex ("Toon", 2D) = "gray" {} 

	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}
		LOD 200

		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _RampTex;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _LightColor0;

			struct appdata{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float2 rampUV : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

				float3 normalDirection = normalize(mul(float4(v.normal,1.0),unity_WorldToObject)); 
				float3 lightDirection = normalize(_WorldSpaceLightPos0);
				o.rampUV.x = saturate(dot(lightDirection,normalDirection));//Lambertの計算　ライトの当たり具合を所得

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 c = _Color * tex2D(_MainTex , i.texcoord);
				fixed4 toonColor = tex2D(_RampTex,float2(i.rampUV.x,i.rampUV.x));//ライトの当たり具合をUVとして扱い、_RampTexから色をsampling
				c *=_LightColor0 * (toonColor*0.5+0.5) + (_LightColor0 * UNITY_LIGHTMODEL_AMBIENT);//見た目合わせで適当に

				return c;
			}

			ENDCG
		}
	}
	Fallback "VertexLit"
}