Shader "Custom/SpriteCandleFlicker"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Flicker Settings)]
        [HDR] _FlickerColor ("Flicker Color", Color) = (1.0, 0.7, 0.3, 1.0)
        _Speed ("Flicker Speed", Range(0.1, 50.0)) = 12.0
        _FlickerIntensity ("Flicker Intensity", Range(0.0, 2.0)) = 0.8
        
        [Header(Circle Mask Settings)]
        _Center ("Circle Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Circle Radius", Range(0.01, 1.0)) = 0.4
        _Feather ("Edge Smoothness", Range(0.01, 1.0)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // Для построения шейдеров под спрайты

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            
            fixed4 _FlickerColor;
            float _Speed;
            float _FlickerIntensity;
            
            float4 _Center;
            float _Radius;
            float _Feather;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            // Алгоритм мягкого мерцания свечи (комбинация волн разной частоты)
            float CandleFlicker(float time)
            {
                float wave1 = sin(time * _Speed) * 0.4;
                float wave2 = sin(time * _Speed * 2.33) * 0.3;
                float wave3 = sin(time * _Speed * 4.11) * 0.2;
                float wave4 = sin(time * _Speed * 7.53) * 0.1;

                // Приводим результат к диапазону [0..1]
                float flicker = (wave1 + wave2 + wave3 + wave4) * 0.5 + 0.5;
                return flicker;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Основная текстура спрайта
                fixed4 c = tex2D(_MainTex, IN.uv) * IN.color;

                // 1. Расчет расстояния от центра
                float dist = distance(IN.uv, _Center.xy);

                // 2. Мягкая круговая маска (1 в центре, 0 за пределами радиуса с учетом сглаживания)
                float circleMask = smoothstep(_Radius, max(0.0, _Radius - _Feather), dist);

                // 3. Вычисление динамической интенсивности свечи
                float flicker = CandleFlicker(_Time.y);
                float flickerMultiplier = 1.0 + (flicker - 0.5) * _FlickerIntensity;

                // 4. Наложение света мерцания на спрайт
                fixed3 lightEffect = _FlickerColor.rgb * circleMask * flickerMultiplier;
                
                // Добавляем цвет света к основному цвету спрайта
                c.rgb += lightEffect * c.a;

                // Преумноженный альфа-канал для правильного Blend One OneMinusSrcAlpha
                c.rgb *= c.a;

                return c;
            }
            ENDHLSL
        }
    }
}