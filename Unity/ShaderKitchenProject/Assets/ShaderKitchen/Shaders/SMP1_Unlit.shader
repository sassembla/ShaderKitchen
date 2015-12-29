﻿Shader "ShaderDic/SMP1_Unlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata //UnityCG.cginc参照
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex); // モデルビュー行列（Object2World）x射影行列　v.vertexをワールド座標系からプロジェクション座標系へ変換します（レンダリングされる画面の座標系）
				o.uv = TRANSFORM_TEX(v.uv, _MainTex); //InspectorのTillingとOffsetを反映するための処理になります。　＞　（UnityCg.cginc参照）　#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)
				//o.uv = v.uv; //こっちにしてみると、TillingとOffsetがどう変わるかわかります。
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);			
				return col;
			}
			ENDCG
		}
	}
}
