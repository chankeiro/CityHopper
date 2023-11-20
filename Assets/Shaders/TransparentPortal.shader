Shader "Custom/TransparentPortal"{
	//show values to edit in inspector


		SubShader{

		// Adding a first depth priming pass so the transparent surfaces show in the right order
		Pass {
			//Cull Off // Turn off backface culling so both sides of the meshes are rendered
			ColorMask 0 
		}
		//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry-1"}

		Pass{
				//don't draw color or depth
				Blend Zero One
				ZWrite Off

				CGPROGRAM
				#include "UnityCG.cginc"

				#pragma vertex vert
				#pragma fragment frag

				struct appdata {
					float4 vertex : POSITION;
				};

				struct v2f {
					float4 position : SV_POSITION;
				};

				v2f vert(appdata v) {
					v2f o;
					//calculate the position in clip space to render the object
					o.position = UnityObjectToClipPos(v.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_TARGET{
					return 0;
				}

				ENDCG
			}
	}
}