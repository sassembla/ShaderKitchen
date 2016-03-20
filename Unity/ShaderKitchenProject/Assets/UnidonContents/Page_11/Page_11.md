##NormalMap
グレイスケールの画像を高さ情報として扱い、
ノーマルマップとして動くよう、実際に実装してみます

[参考　うにばな](http://blog.livedoor.jp/akinow/archives/52387194.html)<br>
[Cg Programming/Unity/Lighting of Bumpy Surfaces](https://en.wikibooks.org/wiki/Cg_Programming/Unity/Lighting_of_Bumpy_Surfaces)<br>

うにばなに詳しいのですが<br>
フェース上のピクセルの座標系での、U方向、V方向の傾きを求めます<br>
U方向はTangent、V方向はBinormalと呼ぶ事にしています<br>
ノーマルMAPはRGBのRをX軸成分（tangent）GをY軸成分(Binormal)<br>
0〜255を0〜1とした場合に、0〜0.5までをマイナス方向、0.5〜1までをプラス方向への角度とみなしています (0.5で垂直)<br>
ということで角度はノーマルMAPをサンプリングすると所得できますから、
まずは変化させる法線を所得しないと始まりません<br>
ということでTangent軸の法線と、Binormal軸の法線を所得する必要が有ります<br>
所得が終わったら、変換行列を作り、サンプリングしたノーマルMAPの座標系を変換<br>
あとは↓の通りで、ここではHalfLambertの光の当て方のところにその数値を突っ込んでいます<br>

```
float4 encodedNormal = tex2D(_BumpMap,_BumpMap_ST.xy * i.uv.xy + _BumpMap_ST.zw);
float3 localCoords = float3(2.0 * encodedNormal.a - 1.0,2.0 * encodedNormal.g - 1.0,0.0);
localCoords.z = sqrt(1.0 - dot(localCoords,localCoords));
float3x3 local2WorldTranspose = float3x3(i.TangentW,i.BinormalW,i.NormalW);
float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
・・・・・・
float3 HalfLambert = saturate(dot(lightDirection,normalDirection)) * 0.5 + 0.5;//Half-Lambert
```


```
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
				float4x4 modelMatrix = _Object2World;
				float4x4 modelMatrixInverse = _World2Object;

				//Tangent軸の所得
				o.TangentW = normalize(mul(modelMatrix,float4(v.tangent.xyz,0.0)).xyz);
				//オブジェクトのNormalの所得
				o.NormalW = normalize(mul(float4(v.normal,0.0),modelMatrixInverse).xyz);
				//Binormal軸の所得
				//軸はオブジェクトのNormalとTangent軸に垂直に交差する軸ということ
				o.BinormalW = normalize(cross(o.NormalW,o.TangentW) * v.tangent.w);

				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float4 encodedNormal = tex2D(_BumpMap,_BumpMap_ST.xy * i.uv.xy + _BumpMap_ST.zw);
				
				//0〜0.5と0.5〜1に角度を分類
				float3 localCoords = float3(2.0 * encodedNormal.a - 1.0,2.0 * encodedNormal.g - 1.0,1.0);
				//変換する3軸の行列を作成
				float3x3 local2WorldTranspose = float3x3(i.TangentW,i.BinormalW,i.NormalW);
				
				//変換行列に対して垂直の角度で変換し
				//ピクセル単位の垂直の所得
				float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
				float3 ViewDirection = normalize( _WorldSpaceCameraPos - i.uv);
				float3 lightDirection = normalize(_WorldSpaceLightPos0);
				float3 HalfLambert = saturate(dot(lightDirection,normalDirection)) * 0.5 + 0.5;//Half-Lambert
				float3 lambertColor = 1.0 * _LightColor0 * HalfLambert;


				fixed4 col = tex2D(_MainTex, i.uv) * float4(lambertColor,1.0);
				
				return col;
			}
			ENDCG
		}
	}
}
```