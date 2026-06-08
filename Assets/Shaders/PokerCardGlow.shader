Shader "Custom/PokerCardGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Float) = 2
        _FillAmount ("Fill Amount", Range(0,1)) = 0
        _Softness ("Softness", Float) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _GlowColor;
            float _GlowIntensity;
            float _FillAmount;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseColor = tex2D(_MainTex, i.uv);

                // Máscara vertical (de abajo hacia arriba)
                float mask = smoothstep(_FillAmount - _Softness, _FillAmount, i.uv.y);

                fixed3 glow = _GlowColor.rgb * mask * _GlowIntensity;

                return fixed4(baseColor.rgb + glow, baseColor.a);
            }
            ENDHLSL
        }
    }
}
