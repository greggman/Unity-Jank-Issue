// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "2d/ThreeCol" {
	Properties {
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color1 ("Replace Black", Color) = (0, 0, 0, 0)
		_Color2 ("Replace Grey", Color) = (0, 0, 0, 0)
		_Color3 ("Replace White", Color) = (0, 0, 0, 0)
	}

	SubShader {

		Tags 
		{ 
			"RenderType" = "Opaque" 
			"Queue" = "Transparent+1" 
		}

		Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha 

		Pass {
			// Name "ColorReplacement3"

			// CGPROGRAM
			// #pragma vertex vert
			// #pragma fragment frag
			// #pragma fragmentoption ARB_precision_hint_fastest
			// #include "UnityCG.cginc"

			// ZWrite Off
			//Blend SrcAlpha OneMinusSrcAlpha 
  
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#include "UnityCG.cginc"

			struct v2f
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};

			v2f vert (appdata_tan v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				return o;
			}

			sampler2D _MainTex;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;

			float4 frag(v2f i) : COLOR
			{
				float4 diffuse = tex2D(_MainTex, i.uv);
				float r = diffuse.r;
				float alpha = diffuse.a;

	
				// Black
				float4 result = _Color1;

				// grey
				if (r > 0.33)
					result = _Color2;

				// White
				if (r > 0.66)
					result = _Color3;

				result.a = alpha;
				return result;
			}
			ENDCG
		}
	}
}