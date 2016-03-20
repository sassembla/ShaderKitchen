##PerlinNoize
様々な演出で使われるPerlinNoizeについてみてみます<br>

参考<br>
[パーリンノイズを理解する](http://postd.cc/understanding-perlin-noise/)<br>

実際の計算はさておき、UnityのMathf.PerlinNoiseがありますので、今回はこれを使って手抜きします<br>
そこ以外は今まで作ってきたものをコピペして合体しただけになります<br>
雰囲気付けにShader内でSin(_Time.x)の値をつかってUVを動かして水面を動かしています<br>

[2013-05-25 フレネル反射率について](http://blog.selfshadow.com/2011/07/22/specular-showdown/)<br>
ここのの一番下にあったのですが<br>

WildWestではガウシアン球を使うことでGPUの処理コストを削減しているそうです<br>
ガウシアン球ってなんですかねｗ<br>

perlin.cs<br>

```
using UnityEngine;
using System.Collections;
 
public class perlin : MonoBehaviour {

	//texture
	public int pixSize = 256;
	public float xOrginal;
	public float yOrginal;
	public float scaleFactor = 4.0F;
	private Texture2D noiseTex;
	private Color[] pix;
	private Renderer rend;

	//octaves perlin
	public int octaves = 6;
	public float persistence = 2.0f;

	void Start() {
		initTexture ();
		CalcNoise(xOrginal,yOrginal,scaleFactor,octaves,persistence);
	}

	void Update() {
		//
	}

	void initTexture(){
		rend = GetComponent<MeshRenderer>();
		noiseTex = new Texture2D(pixSize, pixSize);
		pix = new Color[noiseTex.width * noiseTex.height];
		rend.material.mainTexture = noiseTex;
		rend.material.SetFloat ("_TexSize", pixSize); //Set TextureSize
	}

	void CalcNoise(float xOrg,float yOrg,float scale,int oct,float pers) {
		float y = 0.0F;
		while (y < noiseTex.height) {
			float x = 0.0F;
			while (x < noiseTex.width) {
				float xCoord = xOrg + x / noiseTex.width * scale;
				float yCoord = yOrg + y / noiseTex.height * scale;
				float total = octavePerlin(xCoord,yCoord,oct,pers);
				pix[Mathf.FloorToInt(y * noiseTex.width + x)] = new Color(total, total, total);
				x++;
			}
			y++;
		}
		noiseTex.SetPixels(pix);
		noiseTex.Apply();
	}

	float octavePerlin(float xDat,float yDat,int octavesDat, float persistenceDat){
		float total = 0;
		float frequency = 1;
		float amplitude = 1;
		float maxValue = 0;
		for (var i=0; i<octavesDat; i++) {
			total += Mathf.PerlinNoise(xDat * frequency , yDat * frequency) * amplitude;

			maxValue += amplitude;
			amplitude *= persistenceDat;
			frequency *= 2;
		}
		return total / maxValue;
	}

}
```

Shader<br>

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
				float3 viewDir : TEXCOORD1;	//オブジェクトから視線へのベクトル
				float3 normal : NORMAL;		//オブジェクトの法線
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
