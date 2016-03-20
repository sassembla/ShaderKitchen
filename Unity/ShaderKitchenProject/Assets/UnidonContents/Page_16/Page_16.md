##Fresnel
フレネル反射をみてみます

参考サイト<br>
[フレネルの式](https://ja.wikipedia.org/wiki/フレネルの式)<br>
[その７ 斜めから見ると底が見えない水面（フレネル反射）](http://marupeke296.com/DXPS_PS_No7_FresnelReflection.html)<br>
[2013-05-25 フレネル反射率について](http://d.hatena.ne.jp/hanecci/20130525/p3)<br>
[屈折率データ](http://ww1.tiki.ne.jp/~uri-works/tmp/)<br>

説明は上記サイトが詳しいのでよく読んでもらうとして、はまりポイントだけ説明します<br>
フレネルの式の近似値　Fr(θ)≈F0+(1-F0)(1-cosθ)^5<br>
F0　垂直入射時のフレネル反射係数　＜＜　これはなんでしょう<br>

"垂直入射時"とは、Shader上でその面に垂直に入射したときの反射係数(反射率)という意味です<br>
フレネルの反射係数は0°〜45°、45°〜75°、75°〜90°の範囲でフレネルの反射率が大きく変化していきます<br>
Shilickの式では近似をとるため、0°の時の数字を使うことにしています<br>

では、反射係数とはなんでしょうか<br>
光は物質から物質へ移動する際に屈折します<br>
これを相対屈折率といいます<br>
これと別に、真空から物質へ移動する際の屈折を、絶対屈折率といいます<br>
絶対屈折率は状況により数値が変化することはありません<br>
正確な屈折というわけではありませんが、今回はこちらを使うことにしています<br>
絶対屈折率として地球の大気0.93(F0)　と水0.87(F0)をデフォルトとして使っています<br>

```
Shader "ShaderDic/SMP16_Fresnel"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_Cube("Cubemap",Cube) = ""{}
		_In("In-RefRatio",range(0,1)) = 0.93//入射率　0.93は地球の大気
		_Out("Out-RefRatio",range(0,1)) = 0.87//入射率　0.87は水　つまり空気から水に光が入った時の数字をデフォルトにしています
		_FresnelPow("FresnelPow",Range(0.01,5)) = 5//デフォルト5 近似値から離れるのであまりいじりません。
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}
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
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 uv : TEXCOORD0;
				float3 viewDir : TEXCOORD1;
				//オブジェクトから視線へのベクトル
				
				float3 normal : NORMAL;
				//オブジェクトの法線
			};

			float4 _Color;
			samplerCUBE _Cube;
			float _FresnelPow;
			float _In;
			float _Out;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
				o.uv = reflect(-o.viewDir, v.normal);
				o.uv = mul(UNITY_MATRIX_MV,float4(o.uv,0));
				o.normal = v.normal;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 albedo = _Color;
				fixed4 emission = texCUBE(_Cube,i.uv);

				//r = F0 + (1.0 - F0) * pow(1.0 - saturate(dot(E,H)),5); //Shilick's Fresnel
				// FRESNEL CALCS float fcbias = 0.20373;

	    	    float fcbias = pow((_In - 1) / (_Out+1),2);
				float facing = saturate(1.0 - max(dot( normalize(i.viewDir), normalize(i.normal)), 0.0));
				float refl2Refr = max(fcbias + (1.0-fcbias) * pow(facing, _FresnelPow), 0);
				fixed4 col = (1-refl2Refr) * _Color + refl2Refr * emission;//足し算

				return col;
			}

			ENDCG
		}
	}
}

```


