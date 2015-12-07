Shader "ShaderDic/SMP17_Perlin"
{
     Properties 
     {
        _MainTex ("HeightMap", 2D) = "white" {}
        _TexSize("size",float) = 256
        _ColorMap("Texture" , 2D) = "white" {}
        _Pow("HeightRange",Range(0,100)) = 1
        _SpecColor ("Specular Material Color", Color) = (1,1,1,1) 
        _Shininess("Shininess",Range(1,10)) = 8
        _Cube("Cubemap",Cube) = ""{} //worldmap
		_In("In-RefRatio",range(0,1)) = 0.93//入射率　0.93は地球の大気
		_Out("Out-RefRatio",range(0,1)) = 0.87//入射率　0.87は水　つまり空気から水に光が入った時の数字をデフォルトにしています
		_FresnelPow("FresnelPow",Range(0.01,5)) = 5//デフォルト5 近似値から離れるのであまりいじりません。
		_ScrollVec("Anime normal-x,y/water-x,y",vector) = (1.0,1.0,1.0,1.0)
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
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 viewDir : TEXCOORD1;	//オブジェクトから視線へのベクトル
				float3 TangentW : TEXCOORD2;
				float3 NormalW : TEXCOORD3;
				float3 BinormalW : TEXCOORD4;
				float4 Light:COLOR;
			};

			//specular
			float4 _SpecColor;
			float _Shininess;
			
			//normalMap
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _TexSize;
			sampler2D _ColorMap;
			float4 _ColorMap_ST;
			float _Pow;

			//frenel
			samplerCUBE _Cube;
			float _FresnelPow;
			float _In;
			float _Out;
			
			//Animation
			float4 _ScrollVec;
		
			float4 _LightColor0;// color of light source (from Lighting.cginc)
			
			float3 FindNormal(sampler2D tex, float2 uv, float u,float3 norm) // decoord Hightmap
            {
				float me = tex2D(tex,uv).x;
				float n = tex2D(tex,float2(uv.x,uv.y+1.0/_TexSize)).x;
				float s = tex2D(tex,float2(uv.x,uv.y-1.0/_TexSize)).x;
				float e = tex2D(tex,float2(uv.x+1.0/_TexSize,uv.y)).x;
				float w = tex2D(tex,float2(uv.x-1.0/_TexSize,uv.y)).x;                

				//find perpendicular vector to norm:        
				float3 temp = norm; //a temporary vector that is not parallel to norm
				if(norm.x==1)
					temp.y+=0.5;
				else
					temp.x+=0.5;

				//form a basis with norm being one of the axes:
				float3 perp1 = normalize(cross(norm,temp));
				float3 perp2 = normalize(cross(norm,perp1));

				//use the basis to move the normal in its own space by the offset        
				float3 normalOffset = -u * (((n-me)-(s-me))*perp1 + ((e-me)-(w-me))*perp2);
				norm += normalOffset;
				norm = normalize(norm); 
				
				return norm;
            }
            
			v2f vert (appdata v)
			{
				v2f o;
				
				//tangent world
				float4x4 modelMatrix = _Object2World;
				float4x4 modelMatrixInverse = _World2Object;
				
				o.TangentW = normalize(mul(modelMatrix,float4(v.tangent.xyz,0.0)).xyz);
				o.NormalW = normalize(mul(float4(v.normal,0.0),modelMatrixInverse).xyz);
				o.BinormalW = normalize(cross(o.NormalW,o.TangentW) * v.tangent.w);
				
				float3 lightDirection;
				float atten;
				atten = 1.0;
				
				if(0.0== _WorldSpaceLightPos0.w)
				{
					atten = 1.0;
					lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				}
				else 
				{
					float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - mul(modelMatrix,v.vertex).xyz;
					float distance = length(vertexToLightSource);
					atten = 1.0/distance;
					lightDirection = normalize(vertexToLightSource);
				}
				
				o.Light.rgb = lightDirection;
				o.Light.a = atten;
				o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//UVControll
				float2 ScrollUV0 = i.uv.xy;
				float2 ScrollUV1 = i.uv.xy;
				float timeDat = sin(_Time.x); 
				ScrollUV0.x += _ScrollVec.x * timeDat;
				ScrollUV0.y += _ScrollVec.y * timeDat;
				ScrollUV1.x += _ScrollVec.z * timeDat;
				ScrollUV1.y += _ScrollVec.w * timeDat;
			
				//float3 encordedNormal = FindNormal(_MainTex,_MainTex_ST.xy * i.uv.xy + _MainTex_ST.zw,_Pow,i.NormalW);
				float3 encordedNormal = FindNormal(_MainTex,_MainTex_ST.xy * i.uv.xy + _MainTex_ST.zw,_Pow,i.NormalW);
				float3 localCoords = normalize(2.0 * encordedNormal.rgb - float3(1.0,1.0,1.0));
				
				//??
				//float3x3 local2WorldTranspose = float3x3(i.TangentW,i.BinormalW,i.NormalW);//encoord normalmap
				//float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
				
				float3 normalDirection = localCoords;
				float3 ViewDirection = i.viewDir;
				float3 lightDirection = i.Light.rgb;
				float atten = i.Light.a;
			
				//fresnel
				float2 reflectuv = reflect(-ViewDirection, normalDirection);
				
				float fcbias = pow((_In - 1) / (_Out+1),2);
				float facing = saturate(1.0 - max(dot( normalize(ViewDirection), normalize(normalDirection)), 0.0));
				float refl2Refr = max(fcbias + (1.0-fcbias) * pow(facing, _FresnelPow), 0);
				float4 color =  (1-refl2Refr) * tex2D(_ColorMap, ScrollUV1.xy) + refl2Refr * texCUBE(_Cube,reflectuv);//足し算
			
				//HalfLambert
				float3 HalfLambert = atten * _LightColor0.rgb * color.rgb * (max(0.0,dot(lightDirection,normalDirection)) * 0.5 + 0.5); //Diffuse(half Lambert)
				
				//specular
				float3 specularLight;
				if(dot(normalDirection,lightDirection) < 0.0)
				{
					specularLight = float3(0.0,0.0,0.0);
				}
				else
				{
					specularLight = atten * _LightColor0.rgb * _SpecColor.rgb * pow(max(0.0,dot(reflect(-lightDirection,normalDirection),ViewDirection)),_Shininess);
				}
				
				float4 col;
				col = float4((UNITY_LIGHTMODEL_AMBIENT * color).rgb + HalfLambert + specularLight,1.0);
				
				return col;
				
			}
			ENDCG
		}
	}

     FallBack "Diffuse"
 }