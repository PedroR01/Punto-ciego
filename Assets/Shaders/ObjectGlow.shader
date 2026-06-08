Shader "Custom/ObjectGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Float) = 2

        _Speed ("Sweep Speed", Float) = 1
        _Width ("Sweep Width", Float) = 0.2
        _Softness ("Edge Softness", Float) = 0.1
        _Direction ("Direction (1=Vertical, 0=Horizontal)", Float) = 1

        _CycleDuration ("Cycle Duration", Float) = 3     // tiempo total del loop
        _ActiveTime ("Active Time", Float) = 0.8         // cuánto dura el glow
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

            float _Speed;
            float _Width;
            float _Softness;
            float _Direction;

            float _CycleDuration;
            float _ActiveTime;

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

                float coord = lerp(i.uv.x, i.uv.y, _Direction);

                // Tiempo dentro del ciclo
                float cycleTime = frac(_Time.y / _CycleDuration) * _CycleDuration;

                // Normalizamos SOLO dentro del tiempo activo
                float activePhase = cycleTime / _ActiveTime;

                // Máscara: 1 si estamos dentro de la ventana activa, 0 si no
                float isActive = step(cycleTime, _ActiveTime);

                // Barrido solo cuando está activo
                float center = frac(activePhase * _Speed);

                float dist = abs(coord - center);

                float glowMask = smoothstep(_Width, _Width - _Softness, dist);

                float3 glow = _GlowColor.rgb * glowMask * _GlowIntensity * isActive;

                return fixed4(baseColor.rgb + glow, baseColor.a);
            }
            ENDHLSL
        }
    }
}