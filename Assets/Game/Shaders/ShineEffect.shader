Shader "UI/ShineEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineWidth ("Shine Width", Range(0.0, 1.0)) = 0.2
        _ShineIntensity ("Shine Intensity", Range(0, 5)) = 1
        _ShineSpeed ("Shine Speed", Range(0, 10)) = 2
        _ShineOffset ("Shine Offset", Range(-1,2)) = -1

        _ShineAngle ("Shine Angle (deg)", Range(-90,90)) = 0

        _ShinePeriod ("Shine Period (sec)", Range(0.5, 10)) = 3
        _ShineActiveTime ("Shine Active Time (sec)", Range(0.1, 5)) = 1
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
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            fixed4 _ShineColor;
            float _ShineWidth;
            float _ShineIntensity;
            float _ShineSpeed;
            float _ShineOffset;
            float _ShineAngle;

            float _ShinePeriod;
            float _ShineActiveTime;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.texcoord) * i.color;

                // время цикла
                float cycle = _Time.y * _ShineSpeed + _ShineOffset;
                float shineTime = fmod(cycle, _ShinePeriod);

                // нормированное время внутри активной фазы
                float normTime = shineTime / max(_ShineActiveTime, 0.001);

                // мягкое появление в начале (0.1 сек) и исчезновение в конце (0.1 сек)
                float appear = smoothstep(0.0, 0.1, shineTime);
                float disappear = 1.0 - smoothstep(_ShineActiveTime - 0.1, _ShineActiveTime, shineTime);
                float active = appear * disappear;

                // позиция блика
                float shinePos = frac(normTime);

                // поворот UV
                float2 uv = i.texcoord - 0.5;
                float rad = _ShineAngle * (UNITY_PI / 180.0);
                float s = sin(rad);
                float c = cos(rad);
                float2 ruv = float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c) + 0.5;

                // расстояние до линии блика
                float dist = abs(ruv.x - shinePos);

                // плавная маска блика
                float mask = smoothstep(_ShineWidth, 0.0, dist) * active;

                fixed4 shine = _ShineColor * (mask * _ShineIntensity * baseCol.a);

                return baseCol + shine;
            }
            ENDCG
        }
    }
}
