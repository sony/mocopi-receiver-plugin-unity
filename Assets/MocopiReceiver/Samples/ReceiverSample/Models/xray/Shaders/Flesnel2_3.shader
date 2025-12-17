Shader "Custom/012_Fresnel" {
    //show values to edit in inspector
    Properties{
        _Color("Tint", Color) = (0, 0, 0, 1)
        _MainTex("Texture", 2D) = "white" {}
        _MaskBlend("MaskBlend",Range(0,1)) = 0
        _MaskTex("MaskTexture", 2D) = "white" {}
        _Smoothness("Smoothness", Range(0, 1)) = 0
        _Metallic("Metalness", Range(0, 1)) = 0
        [HDR] _Emission("Emission", color) = (0,0,0)

        _FresnelColor("Fresnel Color", Color) = (1,1,1,1)
        [PowerSlider(4)] _FresnelExponent("Fresnel Exponent", Range(0.25, 4)) = 1
        _Intensity1("Intensity 1",Range(0,2)) = 1
        _Intensity2("Intensity 2",Range(0,2)) = 1

        _ScrollSpeedX("Scroll speed X", float) = 0.5
        _ScrollSpeedY("Scroll speed Y", float) = 0
    }
        SubShader{
            //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
            Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }

            CGPROGRAM

            //the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
            //our surface shader function is called surf and we use the standard lighting model, which means PBR lighting
            //fullforwardshadows makes sure unity adds the shadow passes the shader might need
            #pragma surface surf Standard fullforwardshadows
            #pragma target 3.0
            //#pragma vertex vert
            //#pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            sampler2D _MaskTex;
            half _MaskBlend;

            half _Smoothness;
            half _Metallic;
            half3 _Emission;

            float3 _FresnelColor;
            float _FresnelExponent;
            float _Intensity1;
            float _Intensity2;

            float _ScrollSpeedX;
            float _ScrollSpeedY;


            //input struct which is automatically filled by unity
            struct Input {
                float2 uv_MainTex;
                float2 uv_MaskTex;
                float3 worldNormal;
                float3 viewDir;
                INTERNAL_DATA
            };


            //the surface shader function which sets parameters the lighting function then uses
            void surf(Input i, inout SurfaceOutputStandard o) {
                float2 uv = float2(
                                (i.uv_MainTex.x + _ScrollSpeedX * _Time.y) % 1.0,
                                (i.uv_MainTex.y + _ScrollSpeedY * _Time.y) % 1.0
                            );

                //sample and tint albedo texture
                fixed4 col = tex2D(_MainTex, uv);
                fixed4 maskCol = lerp(1,tex2D(_MaskTex, i.uv_MaskTex),_MaskBlend);
                col *= maskCol;

                float a = _Intensity2 * (1 - saturate(_Intensity1 * dot(i.worldNormal, i.viewDir)));

                o.Albedo = a * (col*_Color).rgb;

                //just apply the values for metalness and smoothness
                o.Metallic = _Metallic;
                o.Smoothness = _Smoothness;

                //get the dot product between the normal and the view direction
                float fresnel = dot(i.worldNormal, i.viewDir);
                //invert the fresnel so the big values are on the outside
                fresnel = saturate(1 - fresnel);

                //raise the fresnel value to the exponents power to be able to adjust it
                fresnel = pow(fresnel, _FresnelExponent);

                //combine the fresnel value with a color
                float3 fresnelColor = fresnel * _FresnelColor;
                //apply the fresnel value to the emission
                o.Emission = a * (_Emission * col + fresnelColor);
                o.Alpha = a;
            }
            ENDCG
        }
            FallBack "Standard"
}