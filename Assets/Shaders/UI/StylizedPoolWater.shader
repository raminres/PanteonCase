Shader "Panteon/UI/Stylized Pool Water"
{
    Properties
    {
        // Standard uGUI properties. _MainTex is supplied by the Water Image's sprite.
        [PerRendererData] _MainTex ("Water Sprite", 2D) = "white" {}
        _Color ("Image Tint", Color) = (1, 1, 1, 1)

        // Packed, UV-aligned texture: R = caustics, G = wave noise, B = water coverage, A = foam.
        _MaskTex ("Packed Water Mask (R Caustics, G Waves, B Water, A Foam)", 2D) = "black" {}

        // A vertical gradient colourises the retained sprite detail instead of flattening it.
        _GradientTopColor ("Gradient Top Color", Color) = (0.36, 0.72, 0.74, 1.0)
        _GradientBottomColor ("Gradient Bottom Color", Color) = (0.10, 0.40, 0.50, 1.0)
        _GradientStrength ("Gradient Strength", Range(0, 1)) = 0.36

        _WaveStrength ("Surface Distortion", Range(0, 0.025)) = 0.003
        _WaveSpeed ("Wave Speed", Range(0, 0.5)) = 0.035
        _WaveDirection ("Wave Direction (XY)", Vector) = (0.8, 0.35, 0, 0)

        // A pseudo normal is reconstructed from the moving G height field for a calm, lit surface.
        _NormalStrength ("Normal Wave Strength", Range(0, 8)) = 2.0
        _SurfaceLightDirection ("Surface Light Direction (XYZ)", Vector) = (-0.35, 0.45, 0.82, 0)
        _SurfaceLightColor ("Surface Light Color", Color) = (0.78, 0.94, 1.0, 1.0)
        _SurfaceLightStrength ("Surface Light Strength", Range(0, 1)) = 0.16

        _CausticsColor ("Caustics Color (A = Intensity)", Color) = (0.78, 0.98, 0.94, 0.24)
        _CausticsStrength ("Caustics Strength", Range(0, 1)) = 0.32
        _CausticsSpeed ("Caustics Speed", Range(0, 0.5)) = 0.045
        _CausticsScale ("Caustics Scale", Range(0.5, 4)) = 1.35

        _FoamColor ("Foam Color (A = Intensity)", Color) = (0.9, 0.98, 1.0, 0.7)
        _FoamStrength ("Foam Strength", Range(0, 1)) = 0.65
        _FoamDistortion ("Foam Noise Distortion", Range(0, 0.05)) = 0.008

        // Standard uGUI stencil, clipping, and alpha-clip properties retain Mask/RectMask2D support.
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
            Name "StylizedPoolWater"

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
            sampler2D _MaskTex;
            fixed4 _Color;
            fixed4 _GradientTopColor;
            fixed4 _GradientBottomColor;
            fixed4 _SurfaceLightColor;
            fixed4 _CausticsColor;
            fixed4 _FoamColor;
            float _GradientStrength;
            float _WaveStrength;
            float _WaveSpeed;
            float4 _WaveDirection;
            float _NormalStrength;
            float4 _SurfaceLightDirection;
            float _SurfaceLightStrength;
            float _CausticsStrength;
            float _CausticsSpeed;
            float _CausticsScale;
            float _FoamStrength;
            float _FoamDistortion;
            float4 _MaskTex_TexelSize;
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
                // The static sample keeps B (coverage) and A (painted foam) aligned to the water sprite.
                // Only the procedural samples below are panned, preventing the pool boundary from swimming.
                fixed4 packedMask = tex2D(_MaskTex, i.uv);
                float waterCoverage = packedMask.b;

                float2 waveDirection = normalize(_WaveDirection.xy + float2(0.0001, 0.0001));
                float2 waveUv = frac(i.uv + waveDirection * (_Time.y * _WaveSpeed));

                // The G channel is treated as a small height field. Two neighbouring samples form a
                // pseudo normal; its XY component provides the self-distortion, and its Z keeps it stable.
                float waveHeight = tex2D(_MaskTex, waveUv).g;
                float waveHeightX = tex2D(_MaskTex, waveUv + float2(_MaskTex_TexelSize.x, 0.0)).g;
                float waveHeightY = tex2D(_MaskTex, waveUv + float2(0.0, _MaskTex_TexelSize.y)).g;
                float2 waveSlope = float2(waveHeight - waveHeightX, waveHeight - waveHeightY);
                float3 surfaceNormal = normalize(float3(waveSlope * _NormalStrength, 1.0));
                float2 refractionOffset = surfaceNormal.xy * _WaveStrength;

                // This is deliberately local refraction: the Water sprite samples itself with a tiny UV shift.
                // It avoids a costly screen grab and remains stable inside a Screen Space Overlay Canvas.
                fixed4 waterColor = tex2D(_MainTex, i.uv + refractionOffset) * i.color;
                float3 gradientColor = lerp(_GradientBottomColor.rgb, _GradientTopColor.rgb, saturate(i.uv.y));
                waterColor.rgb = lerp(waterColor.rgb, waterColor.rgb * gradientColor, _GradientStrength);

                // This replaces the former fake reflection. It is directional surface light calculated
                // from the moving pseudo normal, so highlights follow the apparent shape of the waves.
                float3 lightDirection = normalize(_SurfaceLightDirection.xyz + float3(0.0001, 0.0001, 0.0001));
                float surfaceLight = saturate(dot(surfaceNormal, lightDirection));
                waterColor.rgb += _SurfaceLightColor.rgb * (surfaceLight * _SurfaceLightStrength * waterCoverage);

                // Red contains the hand-authored caustic pattern. It drifts independently of the static mask.
                float2 causticsUv = frac(i.uv * _CausticsScale + waveDirection * (_Time.y * _CausticsSpeed));
                float caustics = tex2D(_MaskTex, causticsUv).r;
                waterColor.rgb += _CausticsColor.rgb * (caustics * _CausticsStrength * _CausticsColor.a * waterCoverage);

                // Alpha contains artist-placed foam. The same G noise now offsets its lookup, making the
                // foam gently drift and break up while B still locks the water boundary in place.
                float foamNoise = waveHeight * 2.0 - 1.0;
                float2 foamOffset = float2(foamNoise, -foamNoise) * _FoamDistortion;
                float foam = tex2D(_MaskTex, i.uv + foamOffset).a * _FoamStrength * _FoamColor.a;
                waterColor.rgb = lerp(waterColor.rgb, _FoamColor.rgb, saturate(foam));

                // The blue water mask is the final boundary. Transparent pixels outside the pool stay invisible.
                waterColor.a *= waterCoverage;

                #ifdef UNITY_UI_CLIP_RECT
                waterColor.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(waterColor.a - 0.001);
                #endif

                return waterColor;
            }
            ENDHLSL
        }
    }
}
