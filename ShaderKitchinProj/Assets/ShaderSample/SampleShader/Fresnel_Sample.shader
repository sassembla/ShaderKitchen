Shader "Gios's Shaders/Defuse Bump frisnel"
{
	Properties{
		_MainTex("Diffuse Texture" , 2D) = "white"{}
		_BumpTex("Normal Map" , 2D) = "bump"{}
		_RimColor("Rim Color" , Color) = (1,1,1,1)
		_RimPower("Rim Power" , Range(0.1,10)) = 3
		_FresnelPower("Shininess" , Range(0.02,3)) = 3
	}
	SubShader {
    Tags {"Rendertype" = "Opaque"}
	    CGPROGRAM
	    #pragma surface surf Lambert
	     
	    struct Input{
	        float2 uv_MainTex;
	        float3 viewDir;
	    };

	    sampler2D _MainTex;
	    sampler2D _BumpTex;
	    float4 _RimColor;
	    float _RimPower;
   	    float _FresnelPower;
	     
	    void surf(Input IN,inout SurfaceOutput o){
	        fixed4 tex = tex2D (_MainTex, IN.uv_MainTex);
	        o.Albedo = tex.rgb;
//	        o.Normal =  UnpackNormal (tex2D (_BumpTex, IN.uv_MainTex));
//	        half rim = 1 - saturate(dot(normalize(IN.viewDir), o.Normal));
//	        o.Emission = _RimColor.rgb * pow(rim, _RimPower);

	        // FRESNEL CALCS float fcbias = 0.20373;
	        float fcbias = 0.20373;
			float facing = saturate(1.0 - max(dot( normalize(IN.viewDir.xyz), normalize(o.Normal)), 0.0));
			float refl2Refr = max(fcbias + (1.0-fcbias) * pow(facing, _FresnelPower), 0);
			o.Albedo = refl2Refr;
	    }
	    ENDCG
	 } 
	Fallback "Diffuse"
}