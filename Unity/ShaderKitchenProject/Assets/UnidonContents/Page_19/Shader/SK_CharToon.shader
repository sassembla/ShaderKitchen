Shader "Unlit/SK_Char2"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1) //ベースカラー
		_ShadowColor ("Shadow Color", Color) = (0.5,0.5,0.5,1) //肌用影色
		_SkinShadow ("SkinShadowColor",Color) = (0.5,0.5,0.5,1) //その他影色
		_MainTex ("Texture", 2D) = "white" {} //メインテクスチャ
		_SkinMask ("SkinMask",2D)= "black" {} //肌とその他のマスク
		_RampTex ("Toon", 2D) = "gray" {} //Toonの影の影響度
		_RimPower("Rim Power", Range(0.02,8.0)) = 0.5 //Rimの強さ
		_RimRange("Rim Range", Range(0.01,8.0)) = 2.0 //Rimの光の飛び具合
		_RimAdjust("Rim Adjust", Range(0.0,-10.0)) = 0.0 //Rimの計算座標お補正
		_RimAlpha("RimAlpha", Range(0.0,1.0)) = 1.0//Rimの薄さ
		_LightVector("LightVec",Vector) = (0.0,0.0,0.0,0.0)//ライトの絶対座標
		_normalBlend("HeadBlend",Range(0.0,1.0)) = 1.0//HeadVectorからの法線とモデルの法線のブレンド
		_HeadVector("HeadLightVec",Vector) = (0.0,0.0,0.0,0.0)//ブレンド用法線の根元（だいたい頭の中心にしておく）
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)//アウトラインの色
		_Outline ("Outline width", Range (0.0, 0.03)) = .005//アウトラインの太さ
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}
		LOD 200

//		Pass{
//			//outline
//			Cull Front
//			CGPROGRAM
//
//			#pragma vertex vert
//			#pragma fragment frag
//			#include "UnityCG.cginc"
//
//			sampler2D _MainTex;
//			fixed4 _MainTex_ST;
//			fixed4 _OutlineColor;
//			fixed _Outline;
//
//			struct appdata{
//				fixed4 vertex : POSITION;
//				fixed2 uv : TEXCOORD0;
//				fixed3 normal : NORMAL;
//			};
//
//			struct v2f{
//				fixed4 pos : SV_POSITION;
//				fixed2 uv : TEXCOORD0;
//			};
//
//			v2f vert(appdata v)
//			{
//				v2f o;
//				o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
//				fixed4 norm = mul(UNITY_MATRIX_IT_MV,v.normal);
//				fixed2 offset = TransformViewToProjection(norm.xy);
//
//				//o.pos.xy += offset*o.pos.z*_Outline;
//				o.pos.xy += offset*_Outline;
//
//				o.uv = TRANSFORM_TEX(v.uv,_MainTex);
//
//				return o;
//			}
//
//			fixed4 frag(v2f i) : SV_Target
//			{
//				fixed4 c = tex2D(_MainTex,i.uv) * _OutlineColor;
//				return c;
//			}
//			ENDCG
//		}


		Pass{
			//Main draw
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _RampTex;
			sampler2D _SkinMask;
			fixed4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _LightVector;
			fixed4 _HeadVector;
			fixed4 _ShadowColor;
			fixed4 _SkinShadow;
			fixed4 _LightColor0;
			fixed _RimPower;
			fixed _RimRange;
			fixed _normalBlend;
			fixed _RimAdjust;
			fixed _RimAlpha;

			struct appdata{
				fixed4 vertex : POSITION;
				fixed2 texcoord : TEXCOORD0;
				fixed3 normal : NORMAL;
			};

			struct v2f{
				fixed4 pos : SV_POSITION;
				fixed2 texcoord : TEXCOORD0;
				fixed3 normal : TEXCOORD1;
				fixed4 viewDir : COLOR;
				fixed4 headVec : COLOR1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

				o.headVec = v.vertex;
				o.viewDir = fixed4(normalize(ObjSpaceViewDir(v.vertex)),0.0);
				o.normal = v.normal;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex , i.texcoord) * _Color;
				fixed4 mask = tex2D(_SkinMask , i.texcoord) * _Color;

//				fixed4 headnormal = i.headVec - _HeadVector;
//				headnormal = ((1 - _normalBlend) * float4(i.normal,0.0)) + (_normalBlend * headnormal);
//				fixed3 normalDirection = normalize(mul(headnormal,_World2Object).xyz);
				fixed3 normalDirection = normalize(mul(float4(i.normal,0.0),_World2Object).xyz);
				fixed rampUV = saturate(dot(_LightVector,normalDirection));

				fixed4 toonColor = tex2D(_RampTex,float2(rampUV,rampUV))*0.5 + 0.5;
//
//				i.viewDir.y += _RimAdjust;
//
//				fixed RimValue = pow(1.0 - dot(i.normal , i.viewDir),_RimRange);
//				fixed4 RimColor = smoothstep(1.0 - _RimPower, 1.0 ,RimValue) * (c * _RimAlpha);
//
//				fixed4 ShadowCol = c * lerp(_ShadowColor,_SkinShadow,mask.r);
//
//				c = lerp(ShadowCol,c,toonColor.r);
//
//				c += RimColor;

				c= toonColor;
				return c;
			}

			ENDCG
		}
	}
	Fallback "VertexLit"
}