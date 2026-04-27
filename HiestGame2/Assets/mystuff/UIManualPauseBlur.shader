Shader "Custom/UIManualPauseBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 2.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragHorizontal
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 fragHorizontal (v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy * _BlurSize;

                fixed4 col = 0;
                col += tex2D(_MainTex, i.uv + float2(-4.0 * texel.x, 0)) * 0.05;
                col += tex2D(_MainTex, i.uv + float2(-3.0 * texel.x, 0)) * 0.09;
                col += tex2D(_MainTex, i.uv + float2(-2.0 * texel.x, 0)) * 0.12;
                col += tex2D(_MainTex, i.uv + float2(-1.0 * texel.x, 0)) * 0.15;
                col += tex2D(_MainTex, i.uv) * 0.18;
                col += tex2D(_MainTex, i.uv + float2(1.0 * texel.x, 0)) * 0.15;
                col += tex2D(_MainTex, i.uv + float2(2.0 * texel.x, 0)) * 0.12;
                col += tex2D(_MainTex, i.uv + float2(3.0 * texel.x, 0)) * 0.09;
                col += tex2D(_MainTex, i.uv + float2(4.0 * texel.x, 0)) * 0.05;

                return col;
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragVertical
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 fragVertical (v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy * _BlurSize;

                fixed4 col = 0;
                col += tex2D(_MainTex, i.uv + float2(0, -4.0 * texel.y)) * 0.05;
                col += tex2D(_MainTex, i.uv + float2(0, -3.0 * texel.y)) * 0.09;
                col += tex2D(_MainTex, i.uv + float2(0, -2.0 * texel.y)) * 0.12;
                col += tex2D(_MainTex, i.uv + float2(0, -1.0 * texel.y)) * 0.15;
                col += tex2D(_MainTex, i.uv) * 0.18;
                col += tex2D(_MainTex, i.uv + float2(0, 1.0 * texel.y)) * 0.15;
                col += tex2D(_MainTex, i.uv + float2(0, 2.0 * texel.y)) * 0.12;
                col += tex2D(_MainTex, i.uv + float2(0, 3.0 * texel.y)) * 0.09;
                col += tex2D(_MainTex, i.uv + float2(0, 4.0 * texel.y)) * 0.05;

                return col;
            }
            ENDCG
        }
    }
}