Shader "UI/CandleDistortion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Distortion Settings)]
        _Speed ("Speed", Range(0.1, 10)) = 3.0
        _Frequency ("Frequency", Range(0.1, 20)) = 5.0
        _Amplitude ("Amplitude", Range(0.0, 0.1)) = 0.03
        _BottomAnchor ("Bottom Anchor (Freeze Bottom)", Range(0.0, 1.0)) = 0.2

        // Обязательные свойства для работы Canvas
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;
            
            float _Speed;
            float _Frequency;
            float _Amplitude;
            float _BottomAnchor;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // Маска: фитиль (основание пламени) остается неподвижным, а кончик гнется
                // smoothstep плавно увеличивает влияние волны от низа к верху UV
                float mask = smoothstep(_BottomAnchor, 1.0, uv.y);

                // Расчет синусоидального смещения
                // Использование двух волн с разной частотой дает более живой, органичный эффект
                float wave1 = sin(_Time.y * _Speed + uv.y * _Frequency);
                float wave2 = cos(_Time.y * (_Speed * 1.5) + uv.y * (_Frequency * 0.8)) * 0.5;
                
                float xOffset = (wave1 + wave2) * _Amplitude * mask;

                // Смещаем текстурные координаты по оси X
                uv.x += xOffset;

                // Сэмплируем текстуру UI
                fixed4 color = tex2D(_MainTex, uv) * IN.color;

                // Поддержка UI Mask (Канвас-маскирование)
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                return color;
            }
            ENDCG
        }
    }
}