// Explanations of the goal of this script in PaintManager.cs
Shader "Custom/PaintableShader"
{
    Properties
    {

        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex("Mask Texture (RGB)", 2D) = "black" {}

        // This three parameters aren't really needed to paint the object
        // they are just standard properties used in any surface shader, that3
        // can also be used here. For instance, you can set a Color with alpha 
        // lower than 1, and paint in a transparent material
        _Color("Color", Color) = (0,0,0,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

    }
    SubShader
    {
        // This tags and definitions let this material to be transparent
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        LOD 100
        ZWrite Off // Sets whether the depth buffer (the buffer that knows which objects are closer or farther to the observer) 
            //contents are updated during rendering.Normally, ZWrite is enabled for opaque objects and disabled for semi - transparent ones.
        Blend SrcAlpha OneMinusSrcAlpha 

        CGPROGRAM
        // surface surf -> Physically based Standard lighting model,
        // fullforwardshadows -> enable shadows on all light types
        // alpha:fade  ->This is for transparency
        #pragma surface surf Standard fullforwardshadows alpha:fade 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MaskTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            // This mask gives more weight to the _MaskTex when its alpha is close to 1
            // i.e, when this mask texture has paint on it, it will appear before the 
            // main texture color.
            fixed4 mask = tex2D(_MaskTex, IN.uv_MainTex);
            c = lerp(c, mask, mask.w); // Interpolationg between c and mask based on mask alpha channel
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
