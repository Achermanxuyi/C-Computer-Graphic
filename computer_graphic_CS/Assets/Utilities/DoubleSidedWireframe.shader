// Code written by Gemini

Shader "Custom/DoubleSidedWireframe"
{
    Properties
    {
        _FillColor("Fill Color", Color) = (0.5, 0.5, 0.5, 0.2)
        _WireColor("Wire Color", Color) = (0, 0, 0, 1)
        // Note: Wire thickness is not easily adjustable in this Built-in RP version
    }

    SubShader
    {
        // Use tags for transparency rendering order
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

        // == PASS 1: RENDER THE TRANSPARENT FILL ==
        Pass
        {
            // Standard transparency setup
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            // Disable culling to render front and back faces
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _FillColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _FillColor;
            }
            ENDCG
        }

        // == PASS 2: RENDER THE WIREFRAME ON TOP (CORRECTED) ==
        Pass
        {
            Blend One Zero 
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _WireColor;

            // Define a struct for this pass to send data from vertex to geometry shader
            struct v2g
            {
                float4 vertex : SV_POSITION;
            };

            // Vertex shader now returns the v2g struct
            v2g vert(float4 vertex : POSITION)
            {
                v2g o;
                o.vertex = UnityObjectToClipPos(vertex);
                return o;
            }

            // Geometry shader now correctly uses the v2g struct
            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout LineStream<v2g> lineStream)
            {
                lineStream.Append(i[0]);
                lineStream.Append(i[1]);
                lineStream.Append(i[2]);
                lineStream.Append(i[0]);
            }

            // Fragment shader is unchanged
            fixed4 frag() : SV_Target
            {
                return _WireColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}