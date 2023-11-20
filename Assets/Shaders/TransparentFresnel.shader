Shader "Custom/TransparentFresnel"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Alpha("Alpha", Range(0,1)) = 1
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _FresnelIntensity("Fresnel Intensity", Range(0,1)) = 1
        _Brightness("Brightness", Range(0,10)) = 1.5
        _Emission("Emission", Range(0,1)) = 0
        //_Glossiness ("Smoothness", Range(0,1)) = 0.5
        //_Metallic ("Metallic", Range(0,1)) = 0.0
        //[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        //[ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        //Dissolve properties
        /*_DissolveTexture("Dissolve Texture", 2D) = "white" {}
        _Amount("Dissolve Amount", Range(0,1)) = 0*/
        
    }


    SubShader
    {
        // Adding a first depth priming pass so the transparent surfaces show in the right order
        Pass {
            Cull Off // Turn off backface culling so both sides of the meshes are rendered
            ColorMask 0
        }

        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
        Cull Off // Turn off backface culling so both sides of the meshes are rendered
        LOD 200

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

        #pragma surface surf Standard interpolateview noforwardadd  alpha:fade 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
        #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF

        sampler2D _MainTex;

        struct Input
        {
            half2 uv_MainTex;
            // Necessary for fresnel effect
            fixed3 worldNormal;
            fixed3 viewDir;
            INTERNAL_DATA
        };

        
        fixed _Emission;
        fixed4 _Color;
        fixed _Alpha;
        fixed _FresnelIntensity;
        fixed _Brightness;

        //fixed _Glossiness;
        //fixed _Metallic;
        //fixed _SpecularHighlights;

        //Dissolve properties
        //sampler2D _DissolveTexture;
        //fixed _Amount;

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


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //Dissolve function
            /*fixed dissolve_value = tex2D(_DissolveTexture, IN.uv_MainTex).r;
            clip(dissolve_value - _Amount);*/
            
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = _Brightness * c.rgb;

            // Metallic and smoothness come from slider variables. We don't use
            // them here, so commented
            /*o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;*/

            // Fresnel effect
            // Fresnel effect changes deppending on the viewer position. Abs normal
            // dots abs view dir to not give more intense values to some orientations
            fixed fresnel = dot(abs(IN.worldNormal), abs(IN.viewDir));
            // Saturate: clamp the value between 0 and 1 so we don't get dark artefacts at the backside
            // 1-: invert the fresnel so the big values are on the outside
            fresnel = saturate(1.1 - fresnel);
            //raise the fresnel value to the exponents power to be able to adjust it
            //fresnel = pow(fresnel, 0.7); // AVOID POW OPERATIONS -> It isn't a big change here
            //combine the fresnel value with a color
            fixed3 color = _Color.xyz;
            fixed3 hsvColor = rgb2hsv(color);
            hsvColor.x = frac(hsvColor.x - 0.1); // Complementary color
            fixed3 complementaryColor = hsv2rgb(hsvColor);
            fixed3 fresnelColor = fresnel * complementaryColor;
            //apply the fresnel value to the emission
            o.Emission = _Emission + _FresnelIntensity*fresnelColor;
            o.Alpha = _Alpha * c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
