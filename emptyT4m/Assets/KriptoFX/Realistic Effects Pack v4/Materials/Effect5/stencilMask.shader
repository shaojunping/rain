Shader "Sky/stencilMask"
{
	SubShader
	{
		Tags { "RenderType"="Opaque"  "Queue"="Geometry-1"}
		LOD 100

		Pass
		{
			ColorMask 0  
			ZWrite Off  
      
			Stencil  
			{  
				Ref 2  
				Comp Always  
				Pass Replace  
			}  
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(1.0, 0, 0, 1.0);
			}
			ENDCG
		}
	}
}
