Shader "Custom/TransparentSimple"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Alpha("Alpha", Range(0,1)) = 1
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Brightness("Brightness", Range(0,5)) = 1.0
        //[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        //[ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0
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

            fixed _Glossiness;
            fixed _Metallic;
            fixed _Brightness;
            //fixed _SpecularHighlights;



            void surf(Input IN, inout SurfaceOutputStandard o)
            {

                // Albedo comes from a texture tinted by color
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = _Brightness*c.rgb;
                // Metallic and smoothness come from slider variables. 
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;

                o.Emission = _Emission;
                o.Alpha = _Alpha;
            }
            ENDCG
        }
        FallBack "Diffuse"
}
