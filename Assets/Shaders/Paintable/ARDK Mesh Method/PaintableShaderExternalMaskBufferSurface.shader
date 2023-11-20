// Explanations of the goal of this script in PaintManagerARDKMesh.cs
// This is the surface version of the shader. Unfortunately, it is not possible to reproduce the glitter
// effect in the AR Mesh, because with the mapping of this shader doesn't work, probably because the UV Mapping
// of the AR chunks are not unwrappled correctly . I didn't investigate it yet. https://youtu.be/YUWfHX_ZNCw?t=67
Shader "Custom/PaintableShaderExternalMaskBufferSurface"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Main Texture (RGB)", 2D) = "white" {}
        _GlitterMask("Glitter Mask Texture (RGB)", 2D) = "black" {} // Hurl Noise 1024x1024. It mush have colors
        _GlitterOffSet("Glitter Offset", Range(0,1)) = 0.5
        _GlitterIntensity("Glitter Intensity", Range(0,2)) = 1.5
        //_FresnelIntensity("Fresnel Intensity", Range(0,1)) = 0.3
    }
    
    SubShader
    {
    
        LOD 100
        ZWrite On // Sets whether the depth buffer (the buffer that knows which objects are closer or farther to the observer) 
            //contents are updated during rendering.Normally, ZWrite is enabled for opaque objects and disabled for semi - transparent ones.
            // ZWrite must be On for transparent objects that must oclude other digital objects (must be opaque for those objects), although
            // they are transparent in AR experiences
        Blend SrcAlpha OneMinusSrcAlpha


        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        // surface surf -> Physically based Standard lighting model,
        // fullforwardshadows -> enable shadows on all light types
        // alpha:fade  ->This is for transparency
        // interpolateview - Compute view direction in the vertex shader and interpolate it; instead of computing it in the pixel shader.
            // This can make the pixel shader faster, but uses up one more texture interpolator.
        //noforwardadd -> makes a shader fully support one directional light in Forward rendering only.The rest of the lights can still
            // have an effect as per - vertex lights or spherical harmonics. This is great to make your shader smaller and make sure it
            // always renders in one pass, even with multiple lights present.
        #pragma surface surf Standard alpha:fade interpolateview noforwardadd

        #include "UnityCG.cginc"

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 3.0

        sampler2D _MainTex;
        sampler2D _GlitterMask;

        fixed4 _Color;
        int _PainterPointsCount;
        // All this array has a size of 5 in order to reduce the work of the shader as much as possible. The teorical maximum would be 1023
        half4 _PainterPositions[5];
        fixed4 _PainterNormals[5];
        fixed _PainterRadius[5];
        fixed4 _PainterColors[5];
        half _PainterTime[5];

        fixed _Hardness;
        fixed _Strength;

        fixed _GlitterOffSet;
        fixed _GlitterIntensity;
        fixed _FresnelIntensity;

        //// Function to calculate angle between vectors
        //float angleBetween(float3 vector1, float3 vector2) {
        //    return acos(dot(vector1, vector2) / (length(vector1) * length(vector2))); // Finding angle between vectors using the dot product
        //}

        // Mask function to calculate which world positions must be paint, based on the distance to the painter
        half maskf(half3 worldPosition, half3 painterPosition, half3 painterPositionNormal, half radius, half hardness) {

            // MATHEMATICAL FUNCTIONS ARE QUITE RESOURCE INTENSIVE IN SHADERS -  NEVER USER THIS
            //// This block generates a pseudo-noise with sin functions of different frecuencies that can be applied to the radius of the mask, so depending
            //// on the angle made by the vector that goes from the painter position to the position to paint (world position), the radius
            //// varies too, to create a kind of "splash" effect. 
            //// The referenceVector is perpendicular to the normal of the painter position, and it is initialized to be the reference to calculate
            //// the angle with the vectos made by all the world positions
            //float3 referenceVector = float3(1, 0, 0) - painterPosition;
            //referenceVector = referenceVector - dot(painterPositionNormal, referenceVector) * painterPositionNormal;
            //float positionAngle = angleBetween(
            //    worldPosition - painterPosition, // Vector between the painter and the point to paint
            //    referenceVector
            //);

            //float angleRadiusNoise1 = 1 + 0.04 * sin(positionAngle * 23);
            //float angleRadiusNoise2 = 1 + 0.06 * sin((positionAngle + painterPosition.x) * 17); // Adding angles with variable values (like painterPosition.x) so the paint looks slightly different each time
            //float angleRadiusNoise3 = 1 + 0.08 * sin((positionAngle + painterPosition.y) * 11);
            //float angleRadiusNoise4 = 1 + 0.1 * sin((positionAngle + painterPosition.z) * 5);
            //float angleRadiusNoise5 = 1 + 0.12 * sin((positionAngle + radius) / 2);
            //float angleRadiusNoise = angleRadiusNoise1 * angleRadiusNoise2 * angleRadiusNoise3 * angleRadiusNoise4 * angleRadiusNoise5;

            //float m = distance(worldPosition, painterPosition) * angleRadiusNoise;

            half m = distance(worldPosition, painterPosition); // This would produce just round paints
            return 1 - smoothstep(radius * hardness, radius, m); // points close to the painter will return values closer to 1 https://thebookofshaders.com/glossary/?search=smoothstep
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

        struct Input
        {
            half2 uv_MainTex;
            half3 worldPos;
            // Necessary for fresnel and glitter effect
            fixed3 worldNormal;
            fixed3 viewDir;
            INTERNAL_DATA
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 col = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed4 glitterMask = tex2D(_GlitterMask, IN.uv_MainTex);

            // The glitter mask will be used as a vector mask that will be multiplied by the view direction vector
            // The direction of the vector on each point of the glitter mask depends on its hue. If we change the hue with time
            // the corresponding vectors will change too, and the same for the multiplication with the direction vector that 
            // procudes the final value of glitter for each point of the mask. Doing so, we get the effect of glitter changing with time.
            fixed3 hsvGlitterMask = rgb2hsv(glitterMask);
            hsvGlitterMask.x = frac(hsvGlitterMask.x + 0.25 * _Time[1]); // Rotation of hue (x value of the vector)
            fixed3 cycleGlitterMask = hsv2rgb(hsvGlitterMask);

            // Glitter
            // Multiply each point of the mask by the abs of the view direction. It must be the abs, because otherwise there are some directions
            // (positive or negative, I am not sure) that get very possitive values and a lot of glitter, and the opposites won't get almost no
            // glitter
            fixed glitter = dot(cycleGlitterMask.xyz, abs(IN.viewDir));
            glitter = saturate(_GlitterOffSet - glitter); // The actual result has values very close to 1, we use the negative to get few values close to 1 + the offset 
            // to regulate a little. Saturate cuts values between 0 and 1. Therefore, higher offset values will produce more glitter, and lower values will produce less glitter
            glitter = pow(glitter, 2 - _GlitterIntensity); // Power to a value lower than 1, to move all values closer to 1, so there aren't many "grey" glitters.

            // Fresnel
            // Fresnel effect changes deppending on the viewer position. Abs normal by abs view dir to not give more intense values to some orientations
            fixed fresnel = dot(abs(IN.worldNormal), abs(IN.viewDir));
            // 1 - fresnel provides values closer to 0 (reducces the places where the fresnel effect is intense and gives more importante to places
            // whose normals are not aligned with our direction (like borders), instead of flat surfaces where we are looking at
            //fresnel = saturate(1 - fresnel);
            //fresnel = pow(fresnel, 1 - _FresnelIntensity); // Regulate the fresnel intensity. AVOID POW (RESOURCE INTENSIVE)

            for (int k = 0; k < (_PainterPointsCount - 1); k++) {
                // Fresnel Color its not the one sent by the painter, but a complementary color (hue difference of 0.1), to create a different effect in the border
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
                fixed f = maskf(IN.worldPos, _PainterPositions[k].xyz, _PainterNormals[k].xyz, _PainterRadius[k], _Hardness);
                fixed edge = f * _Strength;
                col = lerp(col, agCol, edge);
                
            }

            o.Albedo = col.rgb;
            o.Alpha = col.a;


        }
        ENDCG
    }
}
