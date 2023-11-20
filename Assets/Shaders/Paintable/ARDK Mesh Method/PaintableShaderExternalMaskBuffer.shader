// Explanations of the goal of this script in PaintManagerARDKMesh.cs
Shader "Custom/PaintableShaderExternalMaskBuffer"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture (RGB)", 2D) = "white" {}
		_GlitterMask("Glitter Mask Texture (RGB)", 2D) = "black" {} // Hurl Noise 128x128. It mush have colors
		_GlitterOffSet("Glitter Offset", Range(0,1)) = 0.5
		_GlitterIntensity("Glitter Intensity", Range(0,2)) = 1.5
		_DotsReductionTime("Dots Disappearing Time", Range(1,10)) = 5
		_ShadowIntensity("Shadow Intensity", Range(0, 1)) = 0.6
			//_FresnelIntensity("Fresnel Intensity", Range(0,1)) = 0.3
	}
		SubShader
		{
			// OPAQUE MODE - non-transparent for virtual objects
			// Adding a first depth priming pass so the transparent surfaces show in the right order
			Pass {
				ColorMask 0
			}
			

			// These tags and definitions let this material to be transparent
			// Geometry-10 makes the shader render the texture after regular geometry, but before masked geometry and
			// transparent things. The rendering queue determines the order in which objects are rendered, with lower numbers being rendered first.
			Tags {"Queue" = "Geometry-10" "RenderType" = "Transparent"  
			"LightMode" = "ForwardBase" "ForceNoShadowCasting" = "True"}// SHADOW 1. This will be the base forward rendering pass in which ambient, vertex, and
																		// main directional light will be applied. Additional lights will need additional passes
																		// using the "ForwardAdd" lightmode.
			LOD 100
			ZWrite On // Sets whether the depth buffer (the buffer that knows which objects are closer or farther to the observer) 
				//contents are updated during rendering.Normally, ZWrite is enabled for opaque objects and disabled for semi - transparent ones.
				// ZWrite must be On for transparent objects that must oclude other digital objects (must be opaque for those objects), although
				// they are transparent in AR experiences


			// TRANSPARENT MODE - transparent for all objects. Need to use with occlusion
			//Tags {"Queue" = "Geometry+1" "RenderType" = "Transparent" "LightMode" = "ForwardBase" "ForceNoShadowCasting" = "True"}
			//LOD 100
			//ZWrite Off

			Blend SrcAlpha OneMinusSrcAlpha
			Lighting Off // Turn off lighting, because it's expensive and the thing is supposed to be invisible anyway.

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				// SHADOW 2. This matches the "forward base" of the LightMode tag to ensure the shader compiles
				// properly for the forward bass pass. As with the LightMode tag, for any additional lights
				// this would be changed from _fwdbase to _fwdadd.
				#pragma multi_compile_fwdbase
				// SHADOW 3. Reference the Unity library that includes all the lighting shadow macros
				#include "AutoLight.cginc"

				struct appdata
				{
					half4 vertex : POSITION;
					fixed3 normal : NORMAL;
				};

				struct v2f
				{
					half4 worldPos : TEXCOORD0;
					half4 position : SV_POSITION;
					fixed3 normal : NORMAL;
					fixed3 viewDir : TEXCOORD1;
					// SHADOW 4. The LIGHTING_COORDS macro (defined in AutoLight.cginc) defines the parameters needed to sample 
					// the shadow map. The (0,1) specifies which unused TEXCOORD semantics to hold the sampled values - 
					// As we are using texcoords 1 and 2 in this shader we need to use LIGHTING_COORDS(2,3) 
					LIGHTING_COORDS(2, 3)
				};

				// Define all variables here
				sampler2D _MainTex;
				half4 _MainTex_ST;
				sampler2D _GlitterMask;
				half4 _GlitterMask_ST;

				fixed4 _Color;

				int _PainterPointsCount;
				// All these arrays have a size of 5 in order to reduce the work of the shader as much as possible. The teorical maximum would be 1024
				half4 _PainterPositions[5];
				fixed4 _PainterNormals[5];
				fixed _PainterRadius[5];
				fixed4 _PainterColors[5];
				half _PainterTime[5];

				fixed _Hardness;
				fixed _Strength;
				fixed _DotsReductionTime;
				fixed _GlitterOffSet;
				fixed _GlitterIntensity;
				fixed _ShadowIntensity;
				//fixed _FresnelIntensity;

				// Function to calculate angle between vectors
				//float angleBetween(float3 vector1, float3 vector2) {
				//	return acos(dot(vector1, vector2) / (length(vector1) * length(vector2))); // Finding angle between vectors using the dot product
				//}

				// Mask function to calculate which world positions must be paint, based on the distance to the painter
				half maskf(half3 worldPosition,  half3 painterPosition, fixed3 painterPositionNormal, fixed radius, half time, fixed hardness) {

					// MATHEMATICAL FUNCTIONS ARE QUITE RESOURCE INTENSIVE IN SHADERS -  NEVER USER THIS
					//// This block generates a pseudo-noise with sin functions of different frecuencies that can be applied to the radius of the mask, so depending
					//// on the angle made by the vector that goes from the painter position to the position to paint (world position), the radius
					//// varies too, to create a kind of "splash" effect. 
					//// The referenceVector is perpendicular to the normal of the painter position, and it is initialized to be the reference to calculate
					//// the angle with the vectos made by all the world positions
					//float3 referenceVector = float3(1, 0, 0) - painterPosition; 
					//referenceVector = referenceVector - dot(painterPositionNormal, referenceVector) * painterPositionNormal;
					//float positionAngle = angleBetween(
					//	worldPosition - painterPosition, // Vector between the painter and the point to paint
					//	referenceVector
					//);

					//float angleRadiusNoise1 = 1 + 0.04 * sin(positionAngle*23);
					//float angleRadiusNoise2 = 1 + 0.06 * sin((positionAngle + painterPosition.x)*17); // Adding angles with variable values (like painterPosition.x) so the paint looks slightly different each time
					//float angleRadiusNoise3 = 1 + 0.08 * sin((positionAngle + painterPosition.y)*11);
					//float angleRadiusNoise4 = 1 + 0.1 * sin((positionAngle + painterPosition.z)*5);
					//float angleRadiusNoise5 = 1 + 0.12 * sin((positionAngle + radius)/2);
					//float angleRadiusNoise = angleRadiusNoise1 * angleRadiusNoise2 * angleRadiusNoise3 * angleRadiusNoise4 * angleRadiusNoise5;

					//float m = distance(worldPosition, painterPosition) * angleRadiusNoise;

					// The mask gets progresively reduced during X seconds
					half timeReductionRatio = (_Time[1] - time) / _DotsReductionTime;
					fixed m = distance(worldPosition, painterPosition) + radius * timeReductionRatio; // This would produce just round paints that reduce its size with time
					return 1 - smoothstep(radius * hardness, radius , m); // points close to the painter will return values closer to 1 https://thebookofshaders.com/glossary/?search=smoothstep
				}

				// Color transforming functions
				half3 rgb2hsv(half3 c)
				{
					half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
					half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
					half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

					half d = q.x - min(q.w, q.y);
					half e = 1.0e-10;
					return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
				}

				half3 hsv2rgb(half3 c)
				{
					half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
					half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
					return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
				}


				v2f vert(appdata v)
				{
					v2f o;
					//calculate the position in clip space to render the object
					o.position = UnityObjectToClipPos(v.vertex);
					//calculate world position of vertex
					o.worldPos = mul(unity_ObjectToWorld, v.vertex);
					//calculate world normal
					fixed3 worldNormal = mul(v.normal, (fixed3x3)unity_WorldToObject);
					o.normal = normalize(worldNormal);
					// Computes world space view direction
					o.viewDir = WorldSpaceViewDir(v.vertex);
					// SHADOW 5. The TRANSFER_VERTEX_TO_FRAGMENT macro populates the chosen LIGHTING_COORDS in the v2f structure
					// with appropriate values to sample from the shadow/lighting map
					TRANSFER_VERTEX_TO_FRAGMENT(o);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					/* All the _MainTex block is commented because we don't really need
					 an additional texture here for the base color, but we could uncomment it in case it was
					 necessary*/

					 //calculate UV coordinates for three projections
					 /*fixed2 uv_front = TRANSFORM_TEX(i.worldPos.xy, _MainTex);
					 fixed2 uv_side = TRANSFORM_TEX(i.worldPos.zy, _MainTex);
					 fixed2 uv_top = TRANSFORM_TEX(i.worldPos.xz, _MainTex);*/
					 // sample the texture
					 /*fixed4 col_front = tex2D(_MainTex, uv_front);
					 fixed4 col_side = tex2D(_MainTex, uv_side);
					 fixed4 col_top = tex2D(_MainTex, uv_top);*/

					 // Color Noisy Mask for generating the glitter effect
					 //calculate UV coordinates for three projections
					 fixed2 uv_front_mask = TRANSFORM_TEX(i.worldPos.xy, _GlitterMask);
					 fixed2 uv_side_mask = TRANSFORM_TEX(i.worldPos.zy, _GlitterMask);
					 fixed2 uv_top_mask = TRANSFORM_TEX(i.worldPos.xz, _GlitterMask);
					 // sample the texture
					 fixed4 mask_front = tex2D(_GlitterMask, uv_front_mask);
					 fixed4 mask_side = tex2D(_GlitterMask, uv_side_mask);
					 fixed4 mask_top = tex2D(_GlitterMask, uv_top_mask);


					 //generate weights from world normals
					 fixed3 weights = i.normal;
					 //show texture on all directions of the surface (positive and negative normals)
					 weights = abs(weights);
					 //make the transition sharper
					 weights = pow(weights, 32);
					 //make it so the sum of all components is 1
					 weights = weights / (weights.x + weights.y + weights.z);

					 //combine weights with projected colors
					 /*col_front *= weights.z;
					 col_side *= weights.x;
					 col_top *= weights.y;*/

					 //combine weights with projected colors
					 mask_front *= weights.z;
					 mask_side *= weights.x;
					 mask_top *= weights.y;

					 //combine the projected colors 
					 //fixed4 col = (col_front + col_side + col_top);
					 fixed4 glitterMask = (mask_front + mask_side + mask_top);

					 // Multiply by color if selected (transparent for our use case)
					 //col *= _Color; // Uncomment this line and comment the line below in case we want to use _MainTex
					 fixed4 col = _Color;
					 // Calculating these vector only once
					 fixed3 normalizedAbsViewDir = normalize(abs(i.viewDir));

					 // The glitter mask will be used as a vector mask that will be multiplied by the view direction vector
					 // The direction of the vector on each point of the glitter mask depends on its hue. If we change the hue with time
					 // the corresponding vectors will change too, and the same for the multiplication with the direction vector that 
					 // procudes the final value of glitter for each point of the mask. Doing so, we get the effect of glitter changing with time.
					 fixed3 hsvGlitterMask = rgb2hsv(glitterMask);
					 hsvGlitterMask.x = frac(hsvGlitterMask.x + 0.25 * _Time[1]); // Rotation of hue with time (x value of the vector)
					 fixed3 cycleGlitterMask = hsv2rgb(hsvGlitterMask);

					 // Glitter
					 // Multiply each point of the mask by the abs of the view direction. It must be the abs, because otherwise there are some directions
					 // (positive or negative, I am not sure) that get very possitive values and a lot of glitter, and the opposites won't get almost no
					 // glitter
					 fixed glitter = dot(normalize(cycleGlitterMask.xyz), normalizedAbsViewDir);
					 glitter = saturate(_GlitterOffSet - glitter); // The actual result has values very close to 1, we use the negative to get few values close to 1 + the offset 
					 // to regulate a little. Saturate cuts values between 0 and 1. Therefore, higher offset values will produce more glitter, and lower values will produce less glitter
					 glitter = pow(glitter, 2 - _GlitterIntensity); // Power to a value lower than 1, to move all values closer to 1, so there aren't many "grey" glitters.


					 // Fresnel
					 // Fresnel effect changes deppending on the viewer position. Abs normal by abs view dir to not give more intense values to some orientations
					 fixed fresnel = dot(abs(i.normal.xyz), normalizedAbsViewDir);
					 // 1 - fresnel provides values closer to 0 (reducces the places where the fresnel effect is intense and gives more importante to places
					 // whose normals are not aligned with our direction (like borders), instead of flat surfaces where we are looking at. We are not using it
					 //fresnel = saturate(1 - fresnel);
					 //fresnel = pow(fresnel, 1 - _FresnelIntensity); // Regulate the fresnel intensity



					 for (int k = 0; k < (_PainterPointsCount - 1); k++) {
						 // Fresnel Color its not the one sent by the painter, but a complementary color (hue difference of 0.1), to create a different effect in the border
						 // We are not using it here.
						 //fixed3 hsvColor = rgb2hsv(_PainterColors[k]);
						 //hsvColor.x = frac(hsvColor.x - 0.0); // Complementary color
						 //fixed3 complementaryColor = hsv2rgb(hsvColor);
						 //fixed3 fresnelColor = fresnel * complementaryColor;
						 fixed4 fresnelColor = fresnel * _PainterColors[k];

						 // Aggregated color (painter color + glitter effect + fresnel effect
						 fixed4 agCol = _PainterColors[k]
							 + glitter
							 + fresnelColor;
						 //+ fixed4(fresnelColor, _PainterColors[k].w);

						 // Interpolate original main texture color and agColor based on mask functions value
						 fixed f = maskf(i.worldPos, _PainterPositions[k].xyz, _PainterNormals[k].xyz, _PainterRadius[k], _PainterTime[k], _Hardness);
						 fixed edge = f * _Strength;
						 col = lerp(col, agCol, edge);
					 }

					 // SHADOW 6. The LIGHT_ATTENUATION samples the shadowmap (using the coordinates calculated by TRANSFER_VERTEX_TO_FRAGMENT
					 // and stored in the structure defined by LIGHTING_COORDS), and returns the value as a float.
					 // If attenuation < 1 a shadow will be seen. The ShadowStrength paramenter of the Directional Light creating the shadow
					 // directly affects this value, i.e, if the strentgh is 1, the attenuation is 0, but if the strength is 0.4, the attenuation is 0.6.
					 float attenuation = LIGHT_ATTENUATION(i);
					 
					 // If the spots have some alpha, we want to render their color. Otherwise we want that value to be 0, so we can sum it to the
					 // shadow color withouth affecting (we would be summing (0,0,0,0))
					 fixed shadowBalance =  step(0.001, col.a);
					 return  fixed4(0, 0, 0, (1 - attenuation) * _ShadowIntensity) + // This line is just the shadow
						 fixed4(shadowBalance * col.x, shadowBalance * col.y, shadowBalance * col.z, col.a);

				 }
				 ENDCG
			 }
		}
}
