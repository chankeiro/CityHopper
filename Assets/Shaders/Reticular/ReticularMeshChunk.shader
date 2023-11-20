// Based on https://www.ronja-tutorials.com/post/010-triplanar-mapping/

Shader "Unlit/ReticularMeshChunk"
{
  Properties
  {
    _Color("Tint", Color) = (0, 0, 0, 1)
    _MainTex ("Pattern Texture", 2D) = "white" {}
    _MainTextTransparency("Pattern Texture Transparency", Range(0, 1)) = 1.0
        
    // distortion warp stuff
    _ScrollXSpeed("Warp Horizontal Scrolling", Range(-10,10)) = 2
    _ScrollYSpeed("Warp Vertical Scrolling", Range(-10,10)) = 3
    _WarpTex ("Warp Texture", 2D) = "bump" {}
    _WarpStrength ("Warp Strength", float) = 1.0
    _Sharpness("Blend Sharpness", Range(1, 64)) = 1
  }
  SubShader
  {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 100
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"

      struct appdata
      {
        half4 vertex : POSITION;
        fixed3 normal : NORMAL;
      };

      struct v2f
      {
        half3 worldPos : TEXCOORD0;
        half4 position : SV_POSITION;
        fixed3 normal : NORMAL;
        fixed3 viewDir : TEXCOORD1;
        
      };

      sampler2D _MainTex;
      half4 _MainTex_ST;
      fixed _MainTextTransparency;
      sampler2D _MaskTex;
      half4 _MaskTex_ST;
      fixed _ScrollXSpeed;
      fixed _ScrollYSpeed;
            
      sampler2D _WarpTex;
      fixed4 _WarpTex_ST;
      fixed _WarpStrength;
      fixed _Sharpness;
      fixed4 _Color;
      
      v2f vert (appdata v)
      {
        v2f o;
        //calculate the position in clip space to render the object
        o.position  = UnityObjectToClipPos(v.vertex);
        //calculate world position of vertex
        half4 worldPos = mul(unity_ObjectToWorld, v.vertex);
        o.worldPos = worldPos.xyz;
        //calculate world normal
        fixed3 worldNormal = mul(v.normal, (fixed3x3)unity_WorldToObject);
        o.normal = normalize(worldNormal);
        // Computes world space view direction
        o.viewDir = WorldSpaceViewDir(v.vertex);

        return o;
      }
      
      fixed4 frag (v2f i) : SV_Target
      {

         //calculate UV coordinates for three projections
        half2 uv_front = TRANSFORM_TEX(i.worldPos.xy, _MainTex);
        half2 uv_side = TRANSFORM_TEX(i.worldPos.zy, _MainTex);
        half2 uv_top = TRANSFORM_TEX(i.worldPos.xz, _MainTex);
        
        fixed xScrollValue = _ScrollXSpeed * _Time;
        fixed yScrollValue = _ScrollYSpeed * _Time;

        fixed2 scrolledUV_front = TRANSFORM_TEX(i.worldPos.xy, _WarpTex);
        fixed2 scrolledUV_side = TRANSFORM_TEX(i.worldPos.zy, _WarpTex);
        fixed2 scrolledUV_top = TRANSFORM_TEX(i.worldPos.xz, _WarpTex);

        scrolledUV_front += fixed2(xScrollValue, yScrollValue);
        scrolledUV_side += fixed2(xScrollValue, yScrollValue);
        scrolledUV_top += fixed2(xScrollValue, yScrollValue);


        fixed4 warp_front = (tex2D(_WarpTex, scrolledUV_front) - 0.5) * _WarpStrength;
        fixed4 warp_side = (tex2D(_WarpTex, scrolledUV_side) - 0.5) * _WarpStrength;
        fixed4 warp_top = (tex2D(_WarpTex, scrolledUV_top) - 0.5) * _WarpStrength;
        
        // sample the texture
        fixed4 col_front = tex2D(_MainTex, uv_front + warp_front.rg);
        fixed4 col_side = tex2D(_MainTex, uv_side + warp_side.rg);
        fixed4 col_top = tex2D(_MainTex, uv_top + warp_top.rg);
        

        //generate weights from world normals
        fixed3 weights = i.normal;
        //show texture on both sides of the object (positive and negative)
        weights = abs(weights);
        //make the transition sharper
        weights = pow(weights, _Sharpness);
        //make it so the sum of all components is 1
        weights = weights / (weights.x + weights.y + weights.z);

        //combine weights with projected colors
        col_front *= weights.z;
        col_side *= weights.x;
        col_top *= weights.y;
       
        //combine the projected colors 
        fixed4 col = (col_front + col_side + col_top);

        // Providing some additional transparency to the pattern
        col *= fixed4(1, 1, 1, _MainTextTransparency);

        // Adding color to the texture, proportional to the normal direction
        //col = fixed4( abs(i.normal.x), abs(i.normal.z), abs(i.normal.y), 1); // (absolute value provides more clear colors)
        // Changing the order of the vector coordinates to get a different color combination based on the orientation
        //col += fixed4(-1*abs(i.normal.x), -1*abs(i.normal.z), abs(i.normal.y), 1); // lats element regulates intensity
        col += fixed4(i.normal.x, i.normal.z, i.normal.y, 1); // lats element regulates intensity
        // Adding fresnel effect https://docs.unity3d.com/Packages/com.unity.shadergraph@6.9/manual/Fresnel-Effect-Node.html
        //get the dot product between the surface normal and the camera direction
        fixed fresnel = dot(abs(i.normal.xyz), i.viewDir);
        // Saturate: clamp the value between 0 and 1 so we don't get dark artefacts at the backside
        // 1-: invert the fresnel so the big values are on the outside
        fresnel = saturate(1 - fresnel);
        //raise the fresnel value to the exponents power to be able to adjust it
        //fresnel = pow(fresnel, 1); // It seems that doesn't have any effect
        //apply the fresnel value to the color
        col += fresnel;
        // Fresnel - vs 2
        /*float3 I = normalize(i.worldPos - i.viewDir);
        col += 1.0 - saturate(dot(I, normalize(abs(i.normal.xyz))));*/
        

        //multiply texture color with tint color 
        col *= _Color;
        
        return col;
        
        
      }
      ENDCG
    }
  }
}
