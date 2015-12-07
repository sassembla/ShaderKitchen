Shader "ShaderDic/SMP18_Spotlight"
{
     Properties 
     {
        _MainTex ("Main", 2D) = "white" {}
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
				float3 normalDir : TEXCOORD1;
				float4 Light:COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _LightColor0;

			v2f vert (appdata v)
			{
				v2f o;

				//VertexToEye Vector
				
				
				//VertexNormalVector
				o.normalDir = normalize(mul(float4(v.normal,1.0),_World2Object)).xyz;
				//LightVector
				o.Light.rgb = normalize(_WorldSpaceLightPos0.xyz); 
				
				//VertexToLight Vector
				
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				float4 tex = tex2D(_MainTex,i.uv);
				
				float3 normalDirection = i.normalDir;
				float3 lightDirection = i.Light.rgb;
				float atten = 1.0;

				float4 color = tex2D(_MainTex,i.uv);
			
				//HalfLambert
				float3 HalfLambert = atten * _LightColor0.rgb * tex.rgb * (max(0.0,dot(lightDirection,normalDirection)) * 0.5 + 0.5); //Diffuse(half Lambert)

				float4 col = float4((UNITY_LIGHTMODEL_AMBIENT * tex).rgb + HalfLambert,1.0);
		
				return col;
				
			}
			ENDCG
		}
	}

     FallBack "Diffuse"
 }