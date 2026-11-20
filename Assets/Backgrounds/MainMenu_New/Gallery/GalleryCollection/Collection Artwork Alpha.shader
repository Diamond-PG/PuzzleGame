Shader "BAKVAND/UI/ArtworkAlpha"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _AlphaMinimum ("Transparent Below", Range(0, 1)) = 0
        _AlphaSolid ("Opaque Above", Range(0, 1)) = 1
        _FeatherUvRect ("Feather UV Rect", Vector) = (0, 0, 1, 1)
        _TopFeather ("Top Feather", Range(0, 1)) = 0
        _SideFeather ("Side Feather", Range(0, 0.2)) = 0
        _UpperSideInsets ("Upper Side Insets", Vector) = (0, 0, 0, 0)
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
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
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
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 localPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _AlphaMinimum;
            float _AlphaSolid;
            float4 _FeatherUvRect;
            float _TopFeather;
            float _SideFeather;
            float4 _UpperSideInsets;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.localPosition = input.vertex.xy;
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(Varyings input) : SV_Target
            {
                fixed4 sample = tex2D(_MainTex, input.uv) + _TextureSampleAdd;
                float alpha = saturate((sample.a - _AlphaMinimum) / max(_AlphaSolid - _AlphaMinimum, 0.001));
                float2 featherUv = (input.uv - _FeatherUvRect.xy) / max(_FeatherUvRect.zw, 0.0001);
                float feather = _TopFeather > 0 ? smoothstep(0.0, _TopFeather, 1.0 - featherUv.y) : 1.0;
                if (_SideFeather > 0)
                {
                    float2 distances = float2(featherUv.x, 1.0 - featherUv.x)
                        - _UpperSideInsets.xy * featherUv.y;
                    feather *= smoothstep(0.0, _SideFeather, min(distances.x, distances.y));
                }
                sample.a = alpha * feather;
                fixed4 result = sample * input.color;
                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(input.localPosition, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif
                return result;
            }
            ENDCG
        }
    }
}
