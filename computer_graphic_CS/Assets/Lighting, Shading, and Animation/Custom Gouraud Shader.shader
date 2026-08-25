Shader "Unlit/Custom Gouraud Shader"
{
    // Properties
    // {
    //     _MainTex ("Texture", 2D) = "white" {}
    // }
    Properties
    {
        _MAmbient("Material ambient", Color) = (1.0,0.0,0.0,1.0)
        _MEmissive("Material emissive", Color) = (0.0,0.0,0.0,0.0)
        _MDiffuse("Material diffuse", Color) = (1.0,0.0,0.0,1.0)
        _MSpecular("Material specular", Color) = (1.0,1.0,1.0,1.0)
        _MShininess("Material shininess", Float) = 50.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Lighting On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            // #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                // float2 uv : TEXCOORD0;
                float4 normal: NORMAL;
            };

            struct v2f
            {
                // float2 uv : TEXCOORD0;
                // UNITY_FOG_COORDS(1)
                fixed4 c : COLOR;
                float4 vertex : SV_POSITION;
            };

            // sampler2D _MainTex;
            // float4 _MainTex_ST;

            fixed4 _MAmbient;
            fixed4 _MEmissive;
            fixed4 _MDiffuse;
            fixed4 _MSpecular;
            float _MShininess;

            // v2f vert (appdata v)
            // {
            //     v2f o;
            //     o.vertex = UnityObjectToClipPos(v.vertex);
            //     o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            //     UNITY_TRANSFER_FOG(o,o.vertex);
            //     return o;
            // }

            // fixed4 frag (v2f i) : SV_Target
            // {
            //     // sample the texture
            //     fixed4 col = tex2D(_MainTex, i.uv);
            //     // apply fog
            //     UNITY_APPLY_FOG(i.fogCoord, col);
            //     return col;
            // }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float3 P = UnityObjectToViewPos(v.vertex);
                float3 N = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float3 L = unity_LightPosition[0].xyz - P.xyz * unity_LightPosition[0].w;
                        
                // Ambient calculation
                fixed4 ambient = _MAmbient * UNITY_LIGHTMODEL_AMBIENT;

                // Emission calculation
                fixed4 emission = _MEmissive;

                // Diffuse calculation
                float diffuseIntensity = max(dot(N, L), 0);
                fixed4 diffuse = _MDiffuse * unity_LightColor[0] * diffuseIntensity;

                // Specular calculation
                float3 R = normalize(reflect(L, N));
                float3 V = normalize(P);
                float specularIntensity = pow(max(dot(R, V), 0), _MShininess);
                fixed4 specular =_MSpecular * unity_LightColor[0] * specularIntensity;
                o.c = clamp(ambient + emission + diffuse + specular, 0, 1);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.c;
            }
            ENDCG
        }
    }
}
