Shader "Realm of the Lost Puzzle/UI/Level Highlight"
{
    Properties
    {
        [PerRendererData] _MainTex ("Frame distance field", 2D) = "gray" {}
        _ContourColor ("Contour Color", Color) = (0,0.72549,1,1)
        _GlowColor ("Outer Glow Color", Color) = (0,0.39216,1,0.5098)
        _SparkColor ("Energy and Spark Color", Color) = (0.70588,0.96078,1,1)
        _Intensity ("Glow Intensity", Range(0,2)) = 1
        _LineWidth ("Contour Width", Range(0.75,16)) = 8
        _GlowSize ("Outer Glow Size", Range(1,22)) = 18
        _PulsePeriod ("Pulse Period (seconds)", Range(0.25,8)) = 2
        _EnergySpeed ("Energy Speed (laps per second)", Range(0,1)) = 0.24
        _SparkIntensity ("Spark Intensity", Range(0,2)) = 0.85
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "LevelHighlight"
            CGPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                half alpha : COLOR;
                float2 uv : TEXCOORD0;
                half4 mask : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            half4 _ContourColor, _GlowColor, _SparkColor;
            half _Intensity, _LineWidth, _GlowSize, _PulsePeriod, _EnergySpeed, _SparkIntensity;
            float _ROLPLevelHighlightTime;
            float4 _ClipRect;
            float _UIMaskSoftnessX, _UIMaskSoftnessY;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.alpha = input.color.a;
                float2 pixelSize = output.position.w / abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 rect = clamp(_ClipRect, -2e10, 2e10);
                output.mask = half4(input.vertex.xy * 2 - rect.xy - rect.zw,
                    0.25 / (0.25 * float2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize)));
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // The linear mask encodes signed distance from the original alpha edge.
                half distance = (tex2D(_MainTex, input.uv).r - 0.5) * 48;
                // Zero is the silhouette. The visible band crosses it, covering the outer gold edge.
                half outerReach = _LineWidth * 0.6;
                half innerReach = _LineWidth * 0.65;
                half outerFeather = smoothstep(-outerReach - 1, -outerReach + 1, distance);
                half innerFeather = 1 - smoothstep(innerReach - 1, innerReach + 1, distance);
                half contour = outerFeather * innerFeather;

                // The halo radius is measured from the silhouette, within the mask's 24-pixel padding.
                half outerFalloff = 1 - smoothstep(0, _GlowSize, max(0, -distance));
                half halo = outerFalloff * (1 - smoothstep(0, 3, distance));

                float time = _ROLPLevelHighlightTime;
                float2 centred = input.uv - 0.5;
                float angle = atan2(centred.y, centred.x + 0.00001) * 0.15915494 + 0.5;
                half pulse = 0.83 + 0.17 * sin(time * 6.2831853 / _PulsePeriod);
                float phase = angle - time * _EnergySpeed;
                half packets = 1 - smoothstep(0, 0.30, abs(frac(phase * 3) - 0.5));
                half glitter = 1 - smoothstep(0, 0.09, abs(frac(phase * 17 + time * 0.12) - 0.5));
                half ripple = sin(angle * 138.23008 - time * 5.2);
                half filament = 1 - smoothstep(0.4, 1.8, abs(distance + 1.4 + ripple * 1.1));
                half sparks = (packets * filament * 0.75 + glitter * packets *
                    (1 - smoothstep(0.5, 2.1, abs(distance + 4.0 - ripple)))) * _SparkIntensity;

                half outerAlpha = halo * _GlowColor.a * pulse * 0.85;
                half lineAlpha = contour * _ContourColor.a * pulse * 0.60;
                half sparkAlpha = saturate(sparks * _SparkColor.a);
                half opacity = saturate((outerAlpha + lineAlpha + sparkAlpha) * _Intensity);
                half3 rgb = (_GlowColor.rgb * outerAlpha + _ContourColor.rgb * lineAlpha +
                    _SparkColor.rgb * sparkAlpha) * _Intensity;
                half visibility = input.alpha;

                #ifdef UNITY_UI_CLIP_RECT
                    half2 mask = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) * input.mask.zw);
                    visibility *= mask.x * mask.y;
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                    clip(opacity * visibility - 0.001);
                #endif
                return half4(saturate(rgb) * visibility, opacity * visibility);
            }
            ENDCG
        }
    }
}
