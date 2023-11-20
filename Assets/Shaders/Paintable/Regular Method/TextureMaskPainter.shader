// Explanations of the goal of this script in PaintManager.cs
Shader "TNTC/TextureMaskPainter"{

    Properties
    {
        _PainterColor("Painter Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        // Disables culling - all faces of the object are drawn. Used for special effects. Default is Cull Back,
        // which doesn't render  polygons that are facing away from the viewer. I think we could set default here
        // but I am not completely sure
        Cull Off
        // Zwrite Sets whether the depth buffer (the buffer that knows which objects are closer or farther to the observer) 
        //contents are updated during rendering.Normally, ZWrite is enabled for opaque objects and disabled for semi - transparent ones.
        ZWrite Off
        // Sets the conditions under which geometry passes or fails depth testing. Depth testing allows GPUs that have “Early - Z” functionality
        // to reject geometry early in the pipeline, and also ensures correct ordering of the geometry.You can change the conditions 
        // of depth testing to achieve visual effects such as object occlusion. https://programmerclick.com/article/95291383429/
        ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert 
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Mask function properties
            float3 _PainterPosition; 
            float _Radius;
            float _Hardness;
            float _Strength;
            float4 _PainterColor;
            // Mask function
            float mask(float3 worldPosition, float3 painterPosition, float radius, float hardness) {
                float m = distance(painterPosition, worldPosition);
                return 1 - smoothstep(radius * hardness, radius, m); // points close to the painter will return values closer to 1 https://thebookofshaders.com/glossary/?search=smoothstep
            }

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };



            v2f vert(appdata v) {
                v2f o;
                // Provides the worldposition of each point in the screen
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                float4 uv = float4(0, 0, 0, 1);
                // This transformations it is needed to account for different geometry conventions in directX, OpenGl
                // More info here https://youtu.be/YUWfHX_ZNCw?t=80
                uv.xy = float2(1, _ProjectionParams.x)  * (v.uv.xy * float2(2, 2) - float2(1, 1));
                o.vertex = uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target{

                float4 col = tex2D(_MainTex, i.uv); // Picking the MainTex color in each uv position
                // Calculating the f value which will be used to interpolate between the current maintex
                // and the paint color
                float f = mask(i.worldPos, _PainterPosition, _Radius, _Hardness);
                float edge = f * _Strength;
                // Interpolate textures. Values closer to 1 will weight more the paint color
                col = lerp(col, _PainterColor, edge);
                return col;
            }
            ENDCG
        }
    }
}