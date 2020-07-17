// Partially copied from https://medium.com/@onix_systems/how-to-use-360-equirectangular-panoramas-for-greater-realism-in-games-55fadb0547da
Shader "Immersify/Equirectangular Cube BC4 Shader"
{
	Properties
	{
		_TexY("Y texture (R)", 2D) = "white" {}
		_TexU("U Texture (G)", 2D) = "white" {}
		_TexV("V Texture (B)", 2D) = "white" {}
		_TextureTrans("All Texture Transformation", Vector) = (1, 0.5, 0, 0)
		_Visibility("Black to Texture Fade", Range(0.0, 1.0)) = 1
	}

	SubShader
	{
		Tags { "Queue" = "Background" "RenderType" = "Background" }
		Cull Off
		ZWrite Off

		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Fog { Mode off }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _TexY;
			float4 _TexY_ST;
			sampler2D _TexU;
			float4 _TexU_ST;
			sampler2D _TexV;
			float4 _TexV_ST;

			// Using the Texture Trans variable additionally to _Tex*_ST, where x & y are Tiling(x, y) and z & w are Offset(x, y).
			float4 _TextureTrans = float4(1, 0.5, 0, 0);
			float _Visibility = 1;

			struct appdata_t
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
			};

			v2f vert(appdata_t v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.vertex.xyz;

				return o;
			}

			float2 calcUV(float3 texcoord)
			{
				float3 dir = normalize(texcoord);
				float2 longlat = float2(atan2(dir.x, dir.z), acos(-dir.y));
				float2 uv = longlat / float2(2.0 * UNITY_PI, UNITY_PI);
				uv.x += 0.5;

				return uv;
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 uvCoord = calcUV(i.texcoord);
				uvCoord = _TextureTrans.xy * uvCoord + _TextureTrans.zw;

				// Actually, I do not need to use TRANSFORM_TEX(uvCoord, _TexY) but could only use uvCoord, because all calculations are already done with _TextureTrans.
				//float4 yColor = tex2D(_TexY, TRANSFORM_TEX(uvCoord, _TexY));
				//float4 uColor = tex2D(_TexU, TRANSFORM_TEX(uvCoord, _TexU));
				//float4 vColor = tex2D(_TexV, TRANSFORM_TEX(uvCoord, _TexV));
				float4 yColor = tex2D(_TexY, uvCoord);
				float4 uColor = tex2D(_TexU, uvCoord);
				float4 vColor = tex2D(_TexV, uvCoord);

				float r = yColor.r + 1.370705 * (vColor.r - 0.5);
				float g = yColor.r - 0.698001 * (vColor.r - 0.5) - (0.337633 * (uColor.r - 0.5));
				float b = yColor.r + 1.732446 * (uColor.r - 0.5);

				float4 outColor = float4(r, g, b, 1) * _Visibility;

				return outColor;
			}

			ENDCG
		}
	}

	Fallback Off
}
