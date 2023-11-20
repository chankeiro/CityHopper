Shader "Custom/ShadowCaster"
{
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

        // very basic lit pass just for being able to see the object
        // can be removed if you don't want it, though it's probably better to use the Shadows Only option
        Pass {
            Name "ForwardBase"
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                float4 color : TEXCOORD0;
            };

            half4 _LightColor0;

            void vert(appdata_base v, out v2f o)
            {
                o.pos = UnityObjectToClipPos(v.vertex);
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.color = fixed4(ndotl * _LightColor0.rgb, 1.0);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }

        // Pass to render object as a shadow caster
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
            #include "UnityCG.cginc"
            struct v2f {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            v2f vert(appdata_base v)
            {
                // hackity hack hack hack!
                // prevents the bias settings from having any affect on this shader's shadows
                unity_LightShadowBias = float4(0,0,0,0);

                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}