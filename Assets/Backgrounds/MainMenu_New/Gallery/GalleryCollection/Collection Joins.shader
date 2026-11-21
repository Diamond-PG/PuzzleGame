Shader "BAKVAND/UI/Collection Joins"
{
    Properties
    {
        [PerRendererData] _MainTex ("Join Mask", 2D) = "black" {}
        _JoinedEdges ("Joined Edges", Vector) = (0, 0, 0, 0)
        _SeamOpacity ("Seam Opacity", Range(0, 1)) = 0
        _SeamThickness ("Seam Thickness", Range(0.5, 1.6)) = 1
        _CompletionLight ("Wave Center, Wave Intensity, Flash Radius, Flash Brightness", Vector) = (0, 0, 0, 0)
        _FinishingLight ("Sheen Center, Sheen Brightness", Vector) = (0, 0, 0, 0)
        _RevealTexture ("Revealing Fragment", 2D) = "black" {}
        _RevealPose ("Fragment Center X, Y, Inverse Scale", Vector) = (0, 0, 1, 0)
        _RevealLight ("Flash Brightness, Flash Radius, Rim Intensity, Progress", Vector) = (0, 0, 0, 0)
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
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ COLLECTION_PIECE_REVEAL COLLECTION_COMPLETION
            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                half opacity : TEXCOORD1;
            };

            sampler2D _MainTex;
            half4 _JoinedEdges;
            half _SeamOpacity;
            half _SeamThickness;
            half4 _CompletionLight;
            half4 _FinishingLight;

            #ifdef COLLECTION_PIECE_REVEAL
            sampler2D _RevealTexture;
            float4 _RevealTexture_TexelSize;
            float4 _RevealPose;
            half4 _RevealLight;

            half PieceLight(float2 uv)
            {
                float2 offset = uv - _RevealPose.xy;
                half radius = length(offset * float2(1.777, 1));
                half falloff = 1 - smoothstep(0, _RevealLight.y, radius);
                half flash = _RevealLight.x * falloff * falloff;

                float2 fragmentUV = _RevealPose.xy + offset * _RevealPose.z;
                float2 texel = _RevealTexture_TexelSize.xy * 2.2;
                half left = tex2D(_RevealTexture, fragmentUV - float2(texel.x, 0)).a;
                half right = tex2D(_RevealTexture, fragmentUV + float2(texel.x, 0)).a;
                half bottom = tex2D(_RevealTexture, fragmentUV - float2(0, texel.y)).a;
                half top = tex2D(_RevealTexture, fragmentUV + float2(0, texel.y)).a;
                half rim = saturate(abs(left - right) + abs(bottom - top));
                half sweep = 1 - smoothstep(0, 0.23, abs(offset.x - 0.4 * offset.y
                    - lerp(-0.36, 0.36, _RevealLight.w)));
                return max(flash, rim * sweep * _RevealLight.z);
            }
            #endif

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.opacity = input.color.a;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 channels = tex2D(_MainTex, input.uv) * _JoinedEdges;
                half edge = max(max(channels.r, channels.g), max(channels.b, channels.a));
                edge = pow(edge, rcp(_SeamThickness));

                half gold = 0;
                #if defined(COLLECTION_COMPLETION)
                half diagonal = (input.uv.x + (1 - input.uv.y) * 0.65) / 1.65;
                half sweep = 1 - smoothstep(0, 0.15, abs(diagonal - _CompletionLight.x));
                half wave = sweep * sqrt(edge) * _CompletionLight.y;
                half radius = length((input.uv - 0.5) * float2(1.777, 1));
                half halo = 1 - smoothstep(0, max(_CompletionLight.z, 0.001), radius);
                half ring = 1 - smoothstep(0, 0.23, abs(radius - _CompletionLight.z * 0.70));
                half flash = _CompletionLight.w * (0.80 * halo + 0.32 * ring);
                half sheen = _FinishingLight.y * (1 - smoothstep(0, 0.24, abs(diagonal - _FinishingLight.x)));
                gold = min(0.90, max(wave, flash) + sheen);
                #elif defined(COLLECTION_PIECE_REVEAL)
                gold = PieceLight(input.uv);
                #endif
                half dark = _SeamOpacity * edge * (1 - gold);
                half alpha = dark + gold;
                half3 shade = half3(0.09, 0.055, 0.035);
                half3 light = half3(1.0, 0.76, 0.37);
                return half4((shade * dark + light * gold) / max(alpha, 0.001), alpha * input.opacity);
            }
            ENDCG
        }
    }
}
