Shader "Unlit/NewUnlitShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
				float2 p;
				float d;

				for (int n=0; n<_NUMOFPOINTS; n++) {
					p = float2(voronoiPoints[(n * 5)], voronoiPoints[n * 5 + 1]);
					d = distance(p, i.uv - 0.5);
					secondMinDistance = (secondMinDistance > minDistance) * minDistance + (secondMinDistance <= minDistance) * secondMinDistance;
					minDistance = (minDistance < d) * minDistance + (minDistance > d) * d;
					minN = (minDistance == d) * n + (minDistance != d) * minN;
				}

				// sample the texture
				fixed4 col = fixed4(voronoiPoints[(minN * 5 + 2)], voronoiPoints[(minN * 5 + 3)], voronoiPoints[(minN * 5 + 4)], 1);
				col.xyz = col.xyz * (1 - (secondMinDistance ) );

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
