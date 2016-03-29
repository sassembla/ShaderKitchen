using UnityEditor;
using UnityEngine;

[InitializeOnLoad] public static class Tests {

	static Tests () {
		var bindata = Resources.Load("Page_1") as TextAsset;
		// Debug.LogError("読み終わったdata:" + bindata.text);
		// で、これを、htmlに変換する。
		var data = WebViewFunction.MarkdownToRichText(bindata.text);
		
		Debug.LogError("vs rawdata:" + data);
		/*
##Unlit
一番シンプルな、テクスチャを表示するだけのシェーダーです<br>
TextureのタイリングやオフセットをUnityではどう取り扱っているのか確認できます<br>

```
Shader "ShaderDic/SMP1_Unlit"
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
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex); 
		// モデルビュー行列（Object2World）x 射影行列 (v.vertex)
		// ワールド座標系からプロジェクション座標系
		//（レンダリングされる画面の座標系）へ変換します
		
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		//InspectorのTillingとOffsetを反映するための処理になります。 		//(UnityCg.cginc参照)
		// #define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)
		//o.uv = v.uv;
		// ↑こっちにしてみると、TillingとOffsetがどう変わるかわかります
	
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
```
		
		*/
		
		// と、まんまhtmlに変換したもの
		
		/*
		
<h2>Unlit</h2>

<p>一番シンプルな、テクスチャを表示するだけのシェーダーです<br>
TextureのタイリングやオフセットをUnityではどう取り扱っているのか確認できます<br></p>

<p>```
Shader "ShaderDic/SMP1_Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }</p>

<pre><code>SubShader
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
            o.vertex = mul(UNITY_MATRIX_MVP, v.vertex); 
    // モデルビュー行列（Object2World）x 射影行列 (v.vertex)
    // ワールド座標系からプロジェクション座標系
    //（レンダリングされる画面の座標系）へ変換します

            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    //InspectorのTillingとOffsetを反映するための処理になります。      //(UnityCg.cginc参照)
    // #define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)
    //o.uv = v.uv;
    // ↑こっちにしてみると、TillingとOffsetがどう変わるかわかります

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
</code></pre>

<p>}
```</p>
		*/
		
		// それをさらに変換したもの
				
		/*
<size=30>Unlit</size>

<color=#ff0000>一番シンプルな、テクスチャを表示するだけのシェーダーです

TextureのタイリングやオフセットをUnityではどう取り扱っているのか確認できます
</color>

<color=#ff0000>```
Shader "ShaderDic/SMP1_Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }</color>

<pre><code>SubShader
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
            o.vertex = mul(UNITY_MATRIX_MVP, v.vertex); 
    // モデルビュー行列（Object2World）x 射影行列 (v.vertex)
    // ワールド座標系からプロジェクション座標系
    //（レンダリングされる画面の座標系）へ変換します

            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    //InspectorのTillingとOffsetを反映するための処理になります。      //(UnityCg.cginc参照)
    // #define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)
    //o.uv = v.uv;
    // ↑こっちにしてみると、TillingとOffsetがどう変わるかわかります

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
</code></pre>

<color=#ff0000>}
```</color>

		*/
	}
}
