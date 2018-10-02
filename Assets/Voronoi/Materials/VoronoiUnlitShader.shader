Shader "Unlit/NewUnlitShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_NUMOFPOINTS ("Number of Voronoi Points", Int) = 24
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			int _NUMOFPOINTS;
			float4 _MainTex_ST;
			float voronoiPoints[5 * 24];

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float minDistance = 10000.0;
				float secondMinDistance = 10000.0;
				int minN = 0;
				int minN2 = 0;
				float2 p;
				float d;

				for (int n=0; n<_NUMOFPOINTS; n++) {
					p = float2(voronoiPoints[(n * 5)], voronoiPoints[n * 5 + 1]);
					d = distance(p, i.uv - 0.5);
					
					minDistance = (minDistance < d) * minDistance + (minDistance > d) * d;
					minN = (minDistance == d) * n + (minDistance != d) * minN;
				}

				bool areweRight = ((i.uv.x - 0.5) > voronoiPoints[(minN * 5)]);
				minN2 = (minN + 1) * areweRight + (minN - 1) * (1 - areweRight);
				minN2 = clamp(minN2, 0, _NUMOFPOINTS);

				secondMinDistance = abs(voronoiPoints[(minN2 * 5)] - (i.uv.x - 0.5));

				float _x = abs((i.uv.x -0.5 - voronoiPoints[(minN * 5)]) / (float)(voronoiPoints[(minN2 * 5)] - voronoiPoints[(minN * 5)])); // abs(x -  min1.x) / abs(min2.x - min1.x)
				_x = 4 * _x * _x * _x;

				fixed4 col = fixed4(voronoiPoints[(minN * 5 + 2)], voronoiPoints[(minN * 5 + 3)], voronoiPoints[(minN * 5 + 4)], 1);
				fixed4 col2 = fixed4(voronoiPoints[(minN2 * 5 + 2)], voronoiPoints[(minN2 * 5 + 3)], voronoiPoints[(minN2 * 5 + 4)], 1);
				col = col * (1-_x) + col2 * (_x);
				col = col * (1-minDistance);

				return col;
			}
			ENDCG
		}
	}
}
