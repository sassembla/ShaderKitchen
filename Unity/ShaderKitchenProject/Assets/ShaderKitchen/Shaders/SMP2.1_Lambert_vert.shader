Shader "ShaderDic/SMP2.1_Lambert_vert"
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
				
				float3 normalDirection = normalize(mul(v.normal,_World2Object)); //Get Vector Face Normal > _World2Object 現在のモデル行列の逆行列
				float3 lightDirection = normalize(_WorldSpaceLightPos0); //Get Vector Light > _WorldSpaceLightPos0 UnityのScene上での一つ目のLightの位置情報
		
				float3 lambert = 1.0 * _LightColor0 * saturate(dot(lightDirection,normalDirection));// Get Lambert
				
				o.color = float4(lambert,1.0);
				
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
