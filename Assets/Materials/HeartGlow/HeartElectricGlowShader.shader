Shader "Custom/HeartElectricGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        [HDR] _CoreColor ("Core Color", Color) = (1.0, 0.82, 0.18, 1.0)
        [HDR] _FlameColor ("Flame Color", Color) = (1.0, 0.28, 0.01, 1.0)
        [HDR] _HotColor ("Hot Color", Color) = (1.0, 0.95, 0.55, 1.0)

        // Сама белая линия
        _CoreStrength ("Core Strength", Range(0, 2)) = 0.22
        _CoreAlpha ("Core Alpha", Range(0, 1)) = 0.25

        // Огненная/электрическая зона
        _FlameStrength ("Flame Strength", Range(0, 10)) = 4.5
        _FlameWidth ("Flame Width", Range(1, 30)) = 12.0
        _OuterFlameWidth ("Outer Flame Width", Range(1, 45)) = 22.0

        // Движение
        _ElectricSpeed ("Electric Speed", Range(0, 20)) = 7.0
        _FlowSpeed ("Flow Speed", Range(0, 20)) = 4.0

        // Хаотичность
        _NoiseScale ("Noise Scale", Range(1, 100)) = 34.0
        _NoiseAmount ("Noise Amount", Range(0, 3)) = 1.4

        // Языки пламени
        _TongueStrength ("Flame Tongue Strength", Range(0, 5)) = 2.0
        _TongueLength ("Flame Tongue Length", Range(0, 3)) = 1.2
        _TongueSharpness ("Flame Tongue Sharpness", Range(1, 20)) = 5.0

        // Вспышки
        _SparkStrength ("Spark Strength", Range(0, 8)) = 3.0
        _SparkSharpness ("Spark Sharpness", Range(1, 25)) = 10.0

        // Мерцание
        _FlickerAmount ("Flicker Amount", Range(0, 1)) = 0.7
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2.0
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.12

        _Alpha ("Overall Alpha", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off

        // Аддитивное свечение
        Blend SrcAlpha One

        Pass
        {
            Name "HeartElectricFlame"

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;

            float4 _CoreColor;
            float4 _FlameColor;
            float4 _HotColor;

            float _CoreStrength;
            float _CoreAlpha;

            float _FlameStrength;
            float _FlameWidth;
            float _OuterFlameWidth;

            float _ElectricSpeed;
            float _FlowSpeed;

            float _NoiseScale;
            float _NoiseAmount;

            float _TongueStrength;
            float _TongueLength;
            float _TongueSharpness;

            float _SparkStrength;
            float _SparkSharpness;

            float _FlickerAmount;
            float _PulseSpeed;
            float _PulseAmount;

            float _Alpha;

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(
                        input.positionOS.xyz
                    );

                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            float Hash21(float2 p)
            {
                p = frac(
                    p *
                    float2(
                        123.34,
                        456.21
                    )
                );

                p += dot(
                    p,
                    p + 45.32
                );

                return frac(
                    p.x * p.y
                );
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                f =
                    f * f *
                    (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));

                return lerp(
                    lerp(a, b, f.x),
                    lerp(c, d, f.x),
                    f.y
                );
            }

            float FBM(float2 p)
            {
                float value = 0.0;

                value +=
                    Noise(p) *
                    0.50;

                p =
                    p * 2.03 +
                    float2(17.1, 9.2);

                value +=
                    Noise(p) *
                    0.25;

                p =
                    p * 2.01 +
                    float2(7.4, 19.7);

                value +=
                    Noise(p) *
                    0.125;

                p =
                    p * 2.07 +
                    float2(13.4, 3.8);

                value +=
                    Noise(p) *
                    0.0625;

                return value;
            }

            float SampleAlpha(float2 uv)
            {
                return SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    uv
                ).a;
            }

            // Расширяет форму контура наружу.
            float DilateAlpha(
                float2 uv,
                float radius
            )
            {
                float2 px =
                    _MainTex_TexelSize.xy *
                    radius;

                float a = 0.0;

                // Центр
                a = max(
                    a,
                    SampleAlpha(uv)
                );

                // Основные направления
                a = max(
                    a,
                    SampleAlpha(
                        uv + float2(px.x, 0)
                    )
                );

                a = max(
                    a,
                    SampleAlpha(
                        uv - float2(px.x, 0)
                    )
                );

                a = max(
                    a,
                    SampleAlpha(
                        uv + float2(0, px.y)
                    )
                );

                a = max(
                    a,
                    SampleAlpha(
                        uv - float2(0, px.y)
                    )
                );

                // Диагонали
                a = max(
                    a,
                    SampleAlpha(
                        uv +
                        float2(
                            px.x * 0.707,
                            px.y * 0.707
                        )
                    )
                );

                a = max(
                    a,
                    SampleAlpha(
                        uv +
                        float2(
                            -px.x * 0.707,
                            px.y * 0.707
                        )
                    )
                );

                a = max(
                    a,
                    SampleAlpha(
                        uv +
                        float2(
                            px.x * 0.707,
                            -px.y * 0.707
                        )
                    )
                );

                a = max(
                    a,
                    SampleAlpha(
                        uv +
                        float2(
                            -px.x * 0.707,
                            -px.y * 0.707
                        )
                    )
                );

                return a;
            }

            half4 frag(
                Varyings input
            ) : SV_Target
            {
                float2 uv =
                    input.uv;

                float time =
                    _Time.y;

                // Исходный белый контур.
                float core =
                    SampleAlpha(uv);

                // Две зоны вокруг контура.
                float innerDilate =
                    DilateAlpha(
                        uv,
                        _FlameWidth
                    );

                float outerDilate =
                    DilateAlpha(
                        uv,
                        _OuterFlameWidth
                    );

                // Убираем саму толстую линию
                // из огненной зоны.
                float innerFlame =
                    saturate(
                        innerDilate -
                        core * 0.92
                    );

                float outerFlame =
                    saturate(
                        outerDilate -
                        innerDilate * 0.65
                    );

                // Движущийся шум.
                float2 noiseUV =
                    uv *
                    _NoiseScale;

                noiseUV.x +=
                    time *
                    _FlowSpeed *
                    0.12;

                noiseUV.y -=
                    time *
                    _ElectricSpeed *
                    0.14;

                float noise1 =
                    FBM(noiseUV);

                float noise2 =
                    FBM(
                        noiseUV * 1.71 +
                        float2(
                            time * 0.73,
                            -time * 0.41
                        )
                    );

                float noise3 =
                    FBM(
                        noiseUV * 2.31 +
                        float2(
                            -time * 0.26,
                            time * 0.91
                        )
                    );

                float movingNoise =
                    saturate(
                        noise1 * 0.45 +
                        noise2 * 0.35 +
                        noise3 * 0.30
                    );

                // Бегущая волна по сердцу.
                float travelling =
                    sin(
                        uv.x * 19.0 +
                        uv.y * 27.0 -
                        time *
                        _ElectricSpeed +
                        movingNoise * 8.0
                    );

                travelling =
                    travelling *
                    0.5 +
                    0.5;

                // Ещё одна волна в другом направлении.
                float travelling2 =
                    sin(
                        uv.x * -31.0 +
                        uv.y * 15.0 +
                        time *
                        _ElectricSpeed *
                        1.37
                    );

                travelling2 =
                    travelling2 *
                    0.5 +
                    0.5;

                float movement =
                    saturate(
                        travelling *
                        travelling2 *
                        1.8
                    );

                // Языки электрического огня.
                float tongueNoise =
                    saturate(
                        movingNoise *
                        _NoiseAmount +
                        movement
                    );

                float tongues =
                    pow(
                        tongueNoise,
                        _TongueSharpness
                    );

                tongues *=
                    _TongueStrength;

                // Более длинная внешняя энергия.
                float outerEnergy =
                    outerFlame *
                    (
                        0.25 +
                        tongues *
                        _TongueLength
                    );

                // Внутренняя огненная зона.
                float innerEnergy =
                    innerFlame *
                    (
                        0.35 +
                        movingNoise *
                        1.25 +
                        movement *
                        0.75
                    );

                // Яркие короткие электрические вспышки.
                float sparkNoise =
                    saturate(
                        noise2 *
                        movement *
                        1.9
                    );

                float sparks =
                    pow(
                        sparkNoise,
                        _SparkSharpness
                    ) *
                    _SparkStrength;

                // Мерцание.
                float flickerNoise =
                    0.45 +
                    noise1 *
                    1.0;

                float flicker =
                    lerp(
                        1.0,
                        flickerNoise,
                        _FlickerAmount
                    );

                // Медленная общая пульсация.
                float pulse =
                    1.0 +
                    sin(
                        time *
                        _PulseSpeed
                    ) *
                    _PulseAmount;

                // Сам контур теперь специально слабый.
                float coreLight =
                    core *
                    _CoreStrength;

                // Основной огонь.
                float flameLight =
                    (
                        innerEnergy +
                        outerEnergy
                    ) *
                    _FlameStrength *
                    flicker *
                    pulse;

                flameLight +=
                    sparks *
                    (
                        innerFlame +
                        outerFlame
                    );

                // Цвет базовой тонкой линии.
                float3 resultColor =
                    _CoreColor.rgb *
                    coreLight;

                // Оранжевый огонь.
                resultColor +=
                    _FlameColor.rgb *
                    flameLight;

                // Самые яркие участки —
                // почти жёлто-белые.
                float hot =
                    saturate(
                        movement *
                        movingNoise *
                        1.8
                    );

                resultColor +=
                    _HotColor.rgb *
                    flameLight *
                    hot *
                    0.65;

                float alpha =
                    (
                        core *
                        _CoreAlpha +
                        innerFlame *
                        0.75 +
                        outerFlame *
                        0.55
                    );

                alpha *=
                    _Alpha *
                    input.color.a;

                return half4(
                    resultColor,
                    saturate(alpha)
                );
            }

            ENDHLSL
        }
    }
}