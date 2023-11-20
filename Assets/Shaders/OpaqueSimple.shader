Shader "Custom/OpaqueSimple"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture (RGB)", 2D) = "white" {}
        _Brightness("Brightness", Range(0,10)) = 1.5
        _Emission("Emission", Range(0,1)) = 0
    }


    SubShader
    {
        
        Tags {"RenderType" = "Opaque" }
        LOD 200


        CGPROGRAM
        // surface surf -> Physically based Standard lighting model,
        
        // fullforwardshadows -> Support all light shadow types in Forward rendering path. By default shaders only support shadows 
        // from one directional light in forward rendering (to save on internal shader variant count).If you need point or Spot Light 
        // shadows in forward rendering, use this directive.
        
        // interpolateview - Compute view direction in the vertex shader and interpolate it; instead of computing it in the pixel shader.
        // This can make the pixel shader faster, but uses up one more texture interpolator.
        
        //noforwardadd -> makes a shader fully support one directional light in Forward rendering only.The rest of the lights can still
        // have an effect as per - vertex lights or spherical harmonics. This is great to make your shader smaller and make sure it
        // always renders in one pass, even with multiple lights present.

        #pragma surface surf Standard interpolateview noforwardadd  

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
        fixed _Brightness;



        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = _Brightness * c.rgb;
            o.Emission = _Emission;

        }
        ENDCG
    }
    FallBack "Diffuse"
}
