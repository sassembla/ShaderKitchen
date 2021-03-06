##Outline
アウトラインをつけてみました<br>
<br>
pass{} で2回書いています<br>
1回目でアウトライン、2回目で本体の描画です<br>
メッシュをちょっと拡大して裏側だけアウトラインカラーで描画した。　という感じです<br>

[Unity のトゥーンシェーダについて調べてみた](http://tips.hecomi.com/entry/2015/04/25/214050)<br>

```
float4 norm = mul(UNITY_MATRIX_IT_MV,v.normal);
float2 offset = TransformViewToProjection(norm.xy);
o.vertex.xy += offset * o.vertex.z * _Outline;
```
この3行でオブジェクトの頂点を法線の向きに、_Outine分移動しています<br>

o.vertex.zにはカメラからの距離が入っています<br>
カメラから遠くなっても近くなっても線の太さを変えないために計算に含めています<br>
この辺も解説が参考のURLに詳しく書いてくれています<br>
カメラから近い時に線が太くて嫌だと思う場合は外すか補正入れてもいいかと思います<br>

```
Shader "ShaderDic/SMP9_Outline"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (.002, 0.03)) = .005
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"} // LightModeを設定
		LOD 100

		Pass
		{
			Cull Front
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
	
			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
			};
		
			struct v2f
			{
				float4 vertex : SV_POSITION;
			};
	
			float _Outline;
			float4 _OutlineColor;
	
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				
				float4 norm = mul(UNITY_MATRIX_IT_MV,v.normal);
				float2 offset = TransformViewToProjection(norm.xy);
				o.vertex.xy += offset * o.vertex.z * _Outline;
				
				return o;
			}
	
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _OutlineColor;
				return col;
			}
			ENDCG
		}
	
		Pass
		{
			Cull Back
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
			
			float _Outline;
			float4 _OutlineColor;
	
			v2f vert (appdata v)
			{
				v2f o;
				
				float3 normalDirection = normalize(mul(v.normal,_World2Object));
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
	Fallback "VertexLit"
}
```

