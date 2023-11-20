Shader "Custom/VideoAlpha"
    {
        Properties
        {
            _MainTex("Video Texture", 2D) = "white"
            _MinDarkness("Float Min Color", Float) = 0
            _MaxDarkness("Float Max Color", Float) = 0.05
        }

        SubShader
        {
           Lighting Off
           Blend One Zero
           // Need the following line because it seems that on some Android phones
           // the Blit() operation (which copies source texture into destination render 
           // texture with a shader) doesn't turn off the depth test.
           ZTest always

           Pass
           {
               CGPROGRAM
               #include "UnityCustomRenderTexture.cginc"
               #pragma vertex CustomRenderTextureVertexShader
               #pragma fragment frag
                #pragma target 3.0

               half4 _Color;
               sampler2D _MainTex;
               half _MinDarkness;
               half _MaxDarkness;

               half4 frag(v2f_customrendertexture IN) : COLOR
               {
                   fixed4 r = tex2D(_MainTex, IN.localTexcoord.xy);
                   //Convert pixels in a darkness range to transparent
                   if ( (r.r + r.g + r.b)/3 >= _MinDarkness & (r.r + r.g + r.b) / 3 <= _MaxDarkness){
                       r.a = 0;
                   }
                   return r;
               }
               ENDCG
            }
        }
    }

