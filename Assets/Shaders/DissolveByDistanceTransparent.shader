// Transparent material with dissolve effect
Shader "Custom/DissolveByDistanceTransparent"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}

        //Dissolve properties
        _DissolveDistance("Dissolve Distance", Range(0,50)) = 0
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

        sampler2D _MainTex;

        fixed4 _Color;

        //Dissolve properties
        half3 _DissolvePosition;
        fixed3 _DissolveNormal;
        fixed _DissolveDistance;

        struct Input
        {
            half2 uv_MainTex;
            half3 worldPos;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Calculate the distance from any surface point to the dissolve location along the dissolve normal
            fixed dist = dot((IN.worldPos - _DissolvePosition.xyz), _DissolveNormal.xyz);
            clip(dist - _DissolveDistance);

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            // But in the limit will the dissolve area it will be white
            fixed dissolveRibet = step(0.0, dist - _DissolveDistance - 0.02);
            o.Albedo = lerp(fixed3(1.0, 1.0, 1.0), c.rgb, dissolveRibet);
            o.Alpha = c.a;
        }
        ENDCG
    }
}
