##Lambert
面の法線から影を計算するLambertシェーダー<br>
VertexShader内で計算する場合とFragment内で計算する場合の違いを確認できます

＜Lambertシェーディング＞<br>
I=αLcosθ<br>
I　反射光<br>
θ　入射光<rb>
α　拡散反射率<br>
L　ライトの強さ<br>

```
//■VertexShader内でランバートを計算
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
			
			float3 normalDirection = normalize(mul(v.normal,_World2Object));
	 //Get Vector Face Normal > _World2Object
	 //現在のモデル行列の逆行列
			float3 lightDirection = normalize(_WorldSpaceLightPos0);
	//Get Vector Light > _WorldSpaceLightPos0
	//UnityのScene上での一つ目のLightの位置情報
			
			float3 lambert = 1.0 * _LightColor0 * saturate(dot(lightDirection,normalDirection));
	// Get Lambert
			
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
```

```
//■FragmentShader内でランバートを計算
Shader "ShaderDic/SMP2.2_Lambert_frag"
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
			float4 vertex : SV_POSITION;
			float4 normal : NORMAL; // normal Direction
			float4 color : COLOR; //light Direction
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		float4 _LightColor0;

		v2f vert (appdata v)
		{
			v2f o;

			o.normal = mul(v.normal,_World2Object);
	//Get Vector Face Normal > _World2Object 
	//現在のモデル行列の逆行列
	
			o.color = _WorldSpaceLightPos0;
	//Get Vector Light > _WorldSpaceLightPos0
	//UnityのScene上での一つ目のLightの位置情報

			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			
			return o;
		}

		fixed4 frag (v2f i) : SV_Target
		{
			float3 normalDirection = normalize(i.normal);
			float3 lightDirection = normalize(i.color);
			
			float3 lambert = 1.0 * _LightColor0 * saturate(dot(normalDirection,lightDirection));
			// Get Lambert
			
			fixed4 col = tex2D(_MainTex, i.uv) * float4(lambert,1.0);

			return col;
		}
		ENDCG
		}
	}
}
```