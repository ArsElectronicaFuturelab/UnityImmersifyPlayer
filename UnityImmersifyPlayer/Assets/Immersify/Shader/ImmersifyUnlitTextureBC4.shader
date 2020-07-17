Shader "Immersify/Unlit Texture BC4"
{
	Properties
	{
		_TexY("Y Texture (R)", 2D) = "white" {}
		_TexU("U Texture (G)", 2D) = "white" {}
		_TexV("U Texture (B)", 2D) = "white" {}
		_Visibility("Black to Texture Fade", Range(0.0, 1.0)) = 1
	}

		SubShader
	{
		Tags { "Queue" = "Background" "RenderType" = "Background" }
		Cull Off
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			sampler2D _TexY;
			float4 _TexY_ST;
			sampler2D _TexU;
			float4 _TexU_ST;
			sampler2D _TexV;
			float4 _TexV_ST;
			float _Visibility = 1;

			v2f vert(appdata_t v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _TexY);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 yColor = tex2D(_TexY, i.texcoord);
				fixed4 uColor = tex2D(_TexU, i.texcoord);
				fixed4 vColor = tex2D(_TexV, i.texcoord);

				float r = yColor.r + 1.370705 * (vColor.r - 0.5);
				float g = yColor.r - 0.698001 * (vColor.r - 0.5) - (0.337633 * (uColor.r - 0.5));
				float b = yColor.r + 1.732446 * (uColor.r - 0.5);

				fixed4 outColor = float4(r,g,b, 1) * _Visibility;

				return outColor;
			}

			ENDCG
		}
	}
}