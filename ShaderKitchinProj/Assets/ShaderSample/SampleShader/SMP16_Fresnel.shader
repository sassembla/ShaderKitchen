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
