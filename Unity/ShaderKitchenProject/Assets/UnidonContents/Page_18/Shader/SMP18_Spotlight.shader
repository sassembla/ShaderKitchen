Shader "ShaderDic/SMP18_Spotlight"
{
     Properties 
     {
        _MainTex ("Main", 2D) = "white" {}
        _LP("LightPos",vector) = (0,1.0,0,1.0)
  	    _LPDir("LightVec",vector) = (0,0,1.0,1.0)
        _SpotCutOff("Corn",range(0.0,1.0))=0.8
     }
 
     SubShader 
     {
         Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}
         LOD 200
         
     
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
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float4 Ver2Eye : TEXCOORD2;
				float4 VerOrigin : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
		
			float4 _LP; // Light World Position
			float4 _LPDir; //Light World Vector
			float _SpotCutOff;

			v2f vert (appdata v)
			{
				v2f o;

				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.VerOrigin = v.vertex;
				o.normal = v.normal;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				float4 tex = tex2D(_MainTex,i.uv);
				
				float3 normalDirection = i.normal;
				//float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz); //ライトの角度 Unity Directional
				
				float4 PositionL =  _LP; 
				float3 Ver2Light = _LP.xyz - mul(_Object2World,i.VerOrigin).xyz; //ピクセルからライトへの方向ベクトル
				float3 lightDirection = _LP.xyz - _LPDir.xyz; //ライトの角度
								
				float Distance = length(Ver2Light);
				float atten = 1.0 / (0.3*Distance*Distance);
				
				//_LightPos.w = 1.0; ベクトルとしての量をライト位置に付与
				//normalize(_LightPos - mul(_Object2World,i.VerOrigin)) ライトまでの方向
				//mul(_Object2World,i.VerOrigin) 頂点の座標
				//_WorldSpaceLightPos0 ライトの角度

				//Spotlight Shadow
				float dat = saturate(dot(lightDirection,normalize(Ver2Light)));

				if(dat < _SpotCutOff){
					dat *= pow(dat/3,12*(0.9-dat))*dat;
				}
				
				return dat;
				
			}
			ENDCG
		}
	}

     FallBack "Diffuse"
 }