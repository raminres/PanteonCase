Shader "Panteon/UI/Statue Gold Shine"
{
    Properties
    {
        // uGUI supplies the sprite texture and per-Image tint through these standard properties.
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Image Tint", Color) = (1, 1, 1, 1)

        _ShineColor ("Shine Color (A = Intensity)", Color) = (1, 0.82, 0.35, 0.65)
        _ShineAngle ("Shine Angle (Degrees)", Range(0, 360)) = 45
        _ShineSpeed ("Shine Speed (UV / Second)", Range(-2, 2)) = 0.16
        _ShineWidth ("Shine Width (UV)", Range(0.01, 0.5)) = 0.08
        _ShineSoftness ("Shine Edge Softness (UV)", Range(0.001, 0.25)) = 0.04

        // Standard uGUI stencil, clipping, and alpha-clip properties preserve Mask/RectMask2D support.
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
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
            Name "StatueGoldShine"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _ShineColor;
            float _ShineAngle;
            float _ShineSpeed;
            float _ShineWidth;
            float _ShineSoftness;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 spriteColor = tex2D(_MainTex, i.uv) * i.color;

                #ifdef UNITY_UI_CLIP_RECT
                spriteColor.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                // Project UVs onto an artist-configurable axis. A moving center position then
                // creates one calm diagonal band with no extra texture sample or screen grab.
                const float radiansPerDegree = 0.01745329252;
                float angleRadians = _ShineAngle * radiansPerDegree;
                float2 direction = float2(cos(angleRadians), sin(angleRadians));
                float projectedUv = dot(i.uv - float2(0.5, 0.5), direction);

                // The projection of a unit UV square is about +/-0.71. Travelling a little
                // beyond it guarantees the shine begins and ends outside the visible sprite.
                float sweepPosition = lerp(-0.9, 0.9, frac(_Time.y * _ShineSpeed));
                float distanceToBand = abs(projectedUv - sweepPosition);
                float band = 1.0 - smoothstep(_ShineWidth, _ShineWidth + _ShineSoftness, distanceToBand);

                // The source alpha is the mask: fully transparent pixels never receive shine.
                // Alpha in Shine Color acts as intensity while preserving normal UI blending.
                spriteColor.rgb += _ShineColor.rgb * (_ShineColor.a * band * spriteColor.a);

                #ifdef UNITY_UI_ALPHACLIP
                clip(spriteColor.a - 0.001);
                #endif

                return spriteColor;
            }
            ENDHLSL
        }
    }
}
