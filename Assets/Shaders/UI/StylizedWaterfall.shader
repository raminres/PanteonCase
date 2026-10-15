Shader "Panteon/UI/Stylized Waterfall"
{
    Properties
    {
        // Standard uGUI inputs. The Image sprite alpha is the waterfall silhouette;
        // a plain white sprite is valid for a rectangular waterfall.
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Image Tint", Color) = (1, 1, 1, 1)

        _TopColor ("Top Color", Color) = (0.35, 0.78, 0.94, 1)
        _BottomColor ("Bottom Color", Color) = (0.08, 0.38, 0.66, 1)
        _Opacity ("Opacity", Range(0, 1)) = 0.8
        _StreakMask ("Waterfall Mask (R = Waves, G = Foam, B = Pulse, A = Shape)", 2D) = "black" {}

        // One shared transform keeps the whole painted effect coherent. Higher tiling values
        // make the R/G/B artwork smaller and more frequent; it never tiles the A silhouette.
        _EffectTiling ("Effect Tiling (X Y)", Vector) = (1, 1, 0, 0)
        _WaveDirection ("Wave Direction (X Y)", Vector) = (0, 1, 0, 0)
        _WaveSpeed ("Wave Speed", Range(-2, 2)) = 0.32
        _NoiseSpeed ("Pulse Noise Speed", Range(-2, 2)) = 0.14
        _NoiseStrength ("Pulse Noise Strength", Range(0, 1)) = 0.5
        _FoamPosition ("Foam Position (UV Y)", Range(-0.5, 0.5)) = 0
        _FoamAmount ("Foam Amount", Range(0, 1)) = 0.55

        // Standard uGUI stencil, clipping, and alpha-clip properties retain
        // compatibility with Mask and RectMask2D.
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
            Name "StylizedWaterfall"

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
            sampler2D _StreakMask;
            fixed4 _Color;
            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _Opacity;
            float4 _EffectTiling;
            float4 _WaveDirection;
            float _WaveSpeed;
            float _NoiseSpeed;
            float _NoiseStrength;
            float _FoamPosition;
            float _FoamAmount;
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
                // Both the assigned Image sprite and A of the packed map define the silhouette.
                // Keeping A in original UV space prevents increased effect tiling from repeating
                // or moving the waterfall's outer contour.
                float sourceAlpha = tex2D(_MainTex, i.uv).a * i.color.a;
                float waterfallShape = tex2D(_StreakMask, i.uv).a;
                float2 effectUv = i.uv * _EffectTiling.xy;
                float2 waveDirection = _WaveDirection.xy;
                float directionLength = max(length(waveDirection), 0.0001);
                waveDirection /= directionLength;

                // R is the artist-authored wave pattern. With the default (0, 1) direction,
                // positive speed moves its features from the top of the Image to the bottom.
                float2 streakUv = effectUv + (_Time.y * _WaveSpeed * waveDirection);
                float streakMask = tex2D(_StreakMask, streakUv).r;
                float2 noiseUv = effectUv + (_Time.y * _NoiseSpeed * waveDirection);
                float pulseNoise = tex2D(_StreakMask, noiseUv).b;
                float layerPulse = lerp(1.0, pulseNoise, _NoiseStrength);

                // The two artist-facing colours form a stable vertical grade. R adds restrained
                // bright streaks, while B makes their strength ebb without changing the silhouette.
                float3 gradientColor = lerp(_BottomColor.rgb, _TopColor.rgb, saturate(i.uv.y));
                float3 color = gradientColor * i.color.rgb;
                float wave = smoothstep(0.18, 0.8, streakMask) * layerPulse;
                color = lerp(color, float3(1.0, 1.0, 1.0) * i.color.rgb, wave * 0.4);

                // G holds the authored foam placement. It deliberately receives no time offset:
                // Foam Position is a static Y adjustment, and B only pulses its visible strength.
                float2 foamUv = effectUv + float2(0.0, _FoamPosition);
                float foamMask = tex2D(_StreakMask, foamUv).g;
                float foam = saturate(foamMask * _FoamAmount * layerPulse);
                color = lerp(color, float3(1.0, 1.0, 1.0) * i.color.rgb, foam);

                fixed4 waterfall = fixed4(color, sourceAlpha * waterfallShape * _Opacity);

                #ifdef UNITY_UI_CLIP_RECT
                waterfall.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(waterfall.a - 0.001);
                #endif

                return waterfall;
            }
            ENDHLSL
        }
    }
}
