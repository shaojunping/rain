// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "TSHD/Billboard/BillBoard_Blend_LocalZ" {
   Properties {
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Texture Image", 2D) = "white" {}
		_ViewerOffset("Viewer offset", float) = 0
		_VerticalBillboarding("Vertical Restraints", Range(0,1)) = 1 
   }
   SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Transparent" "DisableBatching" = "True"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Pass {   
         CGPROGRAM
		 #include "UnityCG.cginc" 
         #pragma vertex vert  
         #pragma fragment frag

         // User-specified uniforms            
         uniform sampler2D _MainTex;  
		  fixed4 _TintColor;

		 float _ViewerOffset,_VerticalBillboarding;

         struct vertexOutput {
            float4 pos : SV_POSITION;
            float4 uv : TEXCOORD0;
         };
		
		void CalcOrthonormalBasis(float3 dir,out float3 right,out float3 up)  
		{  
			up    = abs(dir.y) > 0.999f ? float3(0,0,1) : float3(0,1,0);       
			right = normalize(cross(up,dir));         
			up    = cross(dir,right);     
		} 

         vertexOutput vert(appdata_full v) 
         {
            vertexOutput o;
			float3  centerOffs  = float3(float2(0.5,0.5)- v.color.rg,0);  
			//o.uv = float4(v.color.r, 0.0, 0.0, 1.0);
			float3  centerLocal = v.vertex.xyz + centerOffs.xyz;  
			//centerLocal = v.vertex.xyz;
			float3  viewerLocal = mul(unity_WorldToObject,float4(_WorldSpaceCameraPos,1));              
			float3  localDir    = viewerLocal - centerLocal;  
			//float3  localDir    = viewerLocal -  v.vertex.xyz;  
                  
			localDir.y =localDir.y * _VerticalBillboarding;  
          
			float3  rightLocal;  
			float3  upLocal;  
          
			CalcOrthonormalBasis(normalize(localDir) ,rightLocal,upLocal);  

			float3  BBLocalPos = centerLocal - (rightLocal * centerOffs.x + upLocal * centerOffs.y);      

			BBLocalPos +=_ViewerOffset*localDir;
			o.pos   = UnityObjectToClipPos(float4(BBLocalPos,1)); 
			
			//o.pos =UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;

            return o;
         }
 
         float4 frag(vertexOutput input) : COLOR
         {
			//return input.uv;
			//return input.col*_TintColor;
            return tex2D(_MainTex, float2(input.uv.xy))*_TintColor;   
         }
 
         ENDCG
      }
   }
}