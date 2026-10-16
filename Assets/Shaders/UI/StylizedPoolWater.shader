Shader "Panteon/UI/Stylized Pool Water"
{
    Properties
    {
        // The Image component supplies this sprite. Its alpha is the pool silhouette.
        [PerRendererData] _MainTex ("Water Sprite", 2D) = "white" {}
        _Color ("Image Tint", Color) = (1, 1, 1, 1)

        // Authoring convention: R/G are two caustic states and B/A are two wave states.
        // The paired channels are blended in time instead of being treated as unrelated noise.
        _WaterMask ("TX Caustics And Waves Mask (RG Caustics, BA Waves)", 2D) = "black" {}
        // R is the hand-authored water perimeter and G is the static edge-foam placement.
        _WaterBorderAndNoisesMask ("TX Water Border And Noises Mask (R Perimeter, G Edge Foam)", 2D) = "black" {}

        _TopColor ("Top Color", Color) = (0.30, 0.70, 0.76, 1)
        _BottomColor ("Bottom Color", Color) = (0.08, 0.34, 0.52, 1)
        _Opacity ("Opacity", Range(0, 1)) = 1

        // A UV-aligned copy of the scene. It is flipped vertically before being blended into water.
        _ReflectionTex ("TX Scene Reflection", 2D) = "black" {}
        _ReflectionOpacity ("Reflection Opacity", Range(0, 1)) = 0.28
        _ReflectionFadeDepth ("Reflection Fade Depth", Range(0.01, 1)) = 0.65
        _ReflectionPosition ("Reflection Position (XY)", Vector) = (0, 0, 0, 0)

        // A local, B-channel ring train supplies both visible impact rings and water-detail
        // refraction. This is deliberately not a screen grab: Canvas layers outside this
        // material remain untouched.
        _RippleMask ("TX Ripples Mask (B Ring, G Edge Fade)", 2D) = "black" {}
        _RippleCenter ("Waterfall Ripple Center (XY)", Vector) = (0.499, 0.585, 0, 0)
        _RippleAreaSize ("Waterfall Ripple Area Size (XY)", Vector) = (0.342, 0.220, 0, 0)
        _RippleSize ("Waterfall Ripple Size (X Y)", Vector) = (1, 0.70, 0, 0)
        _RippleSpeed ("Waterfall Ripple Speed", Range(0.01, 1)) = 0.18
        _RippleRingSpawnScale ("Waterfall Ripple Ring Spawn Scale", Range(0.02, 0.5)) = 0.10
        _RippleRingEndScale ("Waterfall Ripple Ring End Scale", Range(0.6, 1.5)) = 1.20
        _RippleRingSpacing ("Waterfall Ripple Ring Spacing", Range(0.12, 0.3)) = 0.20
        _RippleSpawnFadeDuration ("Waterfall Ripple Spawn Fade Duration", Range(0.01, 0.4)) = 0.14
        _RippleRingEndFadeStart ("Waterfall Ripple End Fade Start", Range(0.4, 0.95)) = 0.70
        _RippleHeightStrength ("Waterfall Ripple Height Effect", Range(0, 4)) = 1.25
        _RippleRefractionStrength ("Waterfall Ripple Refraction Strength", Range(0, 0.15)) = 0.025
        _RippleColor ("Waterfall Ripple Color", Color) = (0.78, 0.96, 1, 1)
        _RippleVisualFade ("Waterfall Ripple Visual Fade", Range(0, 1)) = 0.65
        _RippleFoamIntensity ("Waterfall Ripple Foam Intensity", Range(0, 3)) = 0.65
        _RippleFoamRotateSpeed ("Waterfall Ripple Foam Rotate Speed (Degrees/Sec)", Range(-30, 30)) = 2
        _RippleFoamRotationOffset ("Waterfall Ripple Foam Rotation Offset (Degrees)", Range(0, 180)) = 35
        _RippleLightDirection ("Waterfall Ripple Highlight Direction (XY)", Vector) = (-0.55, 0.85, 0, 0)

        // Size is expressed as an intuitive pattern size: larger values make the pattern larger.
        // Perspective Scale is deliberately separate so the existing uniform-size UI slider can
        // remain intact while art direction stretches X and Y independently.
        _CausticsSize ("Caustics Size", Range(0.25, 4)) = 1
        _CausticsPerspectiveScale ("Caustics Perspective Scale (X Y)", Vector) = (1, 1, 0, 0)
        _CausticsColor ("Caustics Color", Color) = (0.72, 0.98, 0.95, 1)
        _CausticsIntensity ("Caustics Intensity", Range(0, 1)) = 0.28
        _CausticsContrast ("Caustics Contrast", Range(0, 4)) = 1
        _CausticsLerpSpeed ("Caustics Lerp Speed", Range(0, 2)) = 0.20

        _WavesSize ("Waves Size", Range(0.25, 4)) = 1
        _WavesColor ("Waves Color", Color) = (0.48, 0.82, 0.91, 1)
        _WavesIntensity ("Waves Intensity", Range(0, 1)) = 0.20
        _WavesLerpSpeed ("Waves Lerp Speed", Range(0, 2)) = 0.26
        _WavesMoveSpeed ("Waves Move Speed", Range(0, 0.2)) = 0.015
        _WavesMoveDirection ("Waves Move Direction (XY)", Vector) = (1, 0.15, 0, 0)

        _FoamColor ("Foam Color", Color) = (0.92, 0.98, 1, 1)
        _FoamIntensity ("Foam Intensity", Range(0, 3)) = 0.7
        _FoamDistortionStrength ("Foam Distortion Strength", Range(0, 0.2)) = 0.006
        _FoamDistortionSpeed ("Foam Distortion Speed", Range(0, 1)) = 0.01
        _FoamDistortionSize ("Foam Distortion Size", Range(0.1, 10)) = 1
        _FoamDistortionDirection ("Foam Distortion Direction (XY)", Vector) = (1, 0, 0, 0)

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
            Name "StylizedPoolWaterFoundation"

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
            sampler2D _WaterMask;
            sampler2D _WaterBorderAndNoisesMask;
            sampler2D _ReflectionTex;
            sampler2D _RippleMask;
            fixed4 _Color;
            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed4 _RippleColor;
            fixed4 _CausticsColor;
            fixed4 _WavesColor;
            float _Opacity;
            float _ReflectionOpacity;
            float _ReflectionFadeDepth;
            float4 _ReflectionPosition;
            float4 _RippleCenter;
            float4 _RippleAreaSize;
            float4 _RippleSize;
            float _RippleSpeed;
            float _RippleRingSpawnScale;
            float _RippleRingEndScale;
            float _RippleRingSpacing;
            float _RippleSpawnFadeDuration;
            float _RippleRingEndFadeStart;
            float _RippleHeightStrength;
            float _RippleRefractionStrength;
            float _RippleVisualFade;
            float _RippleFoamIntensity;
            float _RippleFoamRotateSpeed;
            float _RippleFoamRotationOffset;
            float4 _RippleLightDirection;
            float _CausticsSize;
            float4 _CausticsPerspectiveScale;
            float _CausticsIntensity;
            float _CausticsContrast;
            float _CausticsLerpSpeed;
            float _WavesSize;
            float _WavesIntensity;
            float _WavesLerpSpeed;
            float _WavesMoveSpeed;
            float4 _WavesMoveDirection;
            fixed4 _FoamColor;
            float _FoamIntensity;
            float _FoamDistortionStrength;
            float _FoamDistortionSpeed;
            float _FoamDistortionSize;
            float4 _FoamDistortionDirection;
            float4 _WaterBorderAndNoisesMask_TexelSize;
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

            float2 RotateAroundCentre(float2 uv, float radians)
            {
                float sine = sin(radians);
                float cosine = cos(radians);
                float2 centredUv = uv - 0.5;
                return float2(centredUv.x * cosine - centredUv.y * sine,
                              centredUv.x * sine + centredUv.y * cosine) + 0.5;
            }

            // X drives the subtle refraction field. Y/Z preserve the previous overlay's
            // rotated B/R look for the visible ring and its foam detail. Keeping them in one
            // shader avoids a second transparent UI draw while retaining per-ring variation.
            float3 EvaluateWaterfallRippleRing(float life, float2 localUv, float ringIndex)
            {
                float scale = lerp(_RippleRingSpawnScale, _RippleRingEndScale, smoothstep(0.0, 1.0, life));
                // Per-axis size retains the perspective-friendly oval control previously
                // authored on the separate ripple Image.
                float2 ringSize = max(_RippleSize.xy * scale, float2(0.0001, 0.0001));
                float2 ringUv = (localUv - 0.5) / ringSize + 0.5;
                float2 inBounds = step(float2(0.0, 0.0), ringUv) * step(ringUv, float2(1.0, 1.0));
                float spawnFade = smoothstep(0.0, _RippleSpawnFadeDuration, life);
                float endFade = 1.0 - smoothstep(_RippleRingEndFadeStart, 1.0, life);
                float commonFade = spawnFade * endFade * inBounds.x * inBounds.y;
                float4 heightMask = tex2D(_RippleMask, saturate(ringUv));

                float rotation = radians(_Time.y * _RippleFoamRotateSpeed
                    + ringIndex * _RippleFoamRotationOffset);
                float4 visualMask = tex2D(_RippleMask, saturate(RotateAroundCentre(ringUv, rotation)));
                return float3(heightMask.b, visualMask.b, visualMask.r * visualMask.b) * commonFade;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Keep the source sprite only as a silhouette; the water body is intentionally
                // recoloured by the artist-facing top-to-bottom gradient.
                fixed4 source = tex2D(_MainTex, i.uv);
                float3 waterColor = lerp(_BottomColor.rgb, _TopColor.rgb, saturate(i.uv.y)) * i.color.rgb;

                // Five B-channel wavefronts keep the local impact continuous. G is sampled in
                // stationary local UVs, so only the outside of the ripple area fades away.
                float2 rippleAreaSize = max(_RippleAreaSize.xy, float2(0.0001, 0.0001));
                float2 localRippleUv = (i.uv - _RippleCenter.xy) / rippleAreaSize + 0.5;
                float2 localRippleInBounds = step(float2(0.0, 0.0), localRippleUv) * step(localRippleUv, float2(1.0, 1.0));
                float rippleLife = frac(_Time.y * _RippleSpeed);
                float3 rippleA = EvaluateWaterfallRippleRing(rippleLife, localRippleUv, 0.0);
                float3 rippleB = EvaluateWaterfallRippleRing(frac(rippleLife + _RippleRingSpacing), localRippleUv, 1.0);
                float3 rippleC = EvaluateWaterfallRippleRing(frac(rippleLife + _RippleRingSpacing * 2.0), localRippleUv, 2.0);
                float3 rippleD = EvaluateWaterfallRippleRing(frac(rippleLife + _RippleRingSpacing * 3.0), localRippleUv, 3.0);
                float3 rippleE = EvaluateWaterfallRippleRing(frac(rippleLife + _RippleRingSpacing * 4.0), localRippleUv, 4.0);
                float rippleEdgeFade = tex2D(_RippleMask, saturate(localRippleUv)).g * localRippleInBounds.x * localRippleInBounds.y;
                float rippleHeight = saturate((rippleA.x + rippleB.x + rippleC.x + rippleD.x + rippleE.x) * rippleEdgeFade);
                float rippleVisualHeight = saturate((rippleA.y + rippleB.y + rippleC.y + rippleD.y + rippleE.y) * rippleEdgeFade);
                float rippleFoam = saturate((rippleA.z + rippleB.z + rippleC.z + rippleD.z + rippleE.z) * rippleEdgeFade)
                    * _RippleFoamIntensity;

                // Derivatives turn the procedural height into a small UV offset. The water's
                // silhouette is never moved; only reflection, caustics, waves, and foam samples
                // receive the offset, avoiding a costly Canvas screen grab or edge distortion.
                float2 rippleOffset = float2(ddx(rippleHeight), ddy(rippleHeight))
                    * _RippleHeightStrength * _RippleRefractionStrength;

                // The reflection source is vertically flipped across the water surface. Its UV
                // position is artist-adjustable, while the depth gradient fades it toward the
                // lower (deeper) part of the pool. Bounds prevent shifted UVs from smearing an
                // edge texel across the water when the reflection is repositioned.
                float2 reflectionUv = float2(i.uv.x + rippleOffset.x, 1.0 - i.uv.y - rippleOffset.y) + _ReflectionPosition.xy;
                float2 reflectionInBounds = step(float2(0.0, 0.0), reflectionUv) * step(reflectionUv, float2(1.0, 1.0));
                fixed4 reflection = tex2D(_ReflectionTex, saturate(reflectionUv));
                float reflectionFade = smoothstep(1.0 - _ReflectionFadeDepth, 1.0, saturate(i.uv.y));
                float reflectionAmount = reflection.a * reflectionInBounds.x * reflectionInBounds.y * reflectionFade * _ReflectionOpacity;
                waterColor = lerp(waterColor, reflection.rgb, saturate(reflectionAmount));

                // R/G are two authored caustic snapshots. The sine returns 0..1 so the blend
                // travels smoothly from R to G and back without introducing a directional pan.
                float causticsBlend = 0.5 + 0.5 * sin(_Time.y * _CausticsLerpSpeed);
                float2 causticsSize = max(_CausticsSize * _CausticsPerspectiveScale.xy,
                                           float2(0.0001, 0.0001));
                float2 causticsUv = frac((i.uv + rippleOffset) / causticsSize);
                float2 causticChannels = tex2D(_WaterMask, causticsUv).rg;
                float caustics = lerp(causticChannels.r, causticChannels.g, causticsBlend);
                // A centred contrast curve preserves the 0.5 midpoint: values above it become
                // brighter while values below it recede, giving the mask clearer stylized peaks.
                caustics = saturate((caustics - 0.5) * _CausticsContrast + 0.5);
                waterColor = lerp(waterColor, _CausticsColor.rgb, saturate(caustics * _CausticsIntensity));

                // B/A use the same two-state blend. Unlike caustics, their lookup receives a
                // very small directional pan, providing the requested subtle surface movement.
                float directionLength = max(length(_WavesMoveDirection.xy), 0.0001);
                float2 waveDirection = _WavesMoveDirection.xy / directionLength;
                float2 wavesUv = frac((i.uv + rippleOffset) / max(_WavesSize, 0.0001) + waveDirection * (_Time.y * _WavesMoveSpeed));
                float wavesBlend = 0.5 + 0.5 * sin(_Time.y * _WavesLerpSpeed);
                float2 waveChannels = tex2D(_WaterMask, wavesUv).ba;
                float waves = lerp(waveChannels.x, waveChannels.y, wavesBlend);
                waterColor = lerp(waterColor, _WavesColor.rgb, saturate(waves * _WavesIntensity));

                // R and G stay UV-aligned to the painted pool. Only B pans as a height field;
                // neighbouring samples form a local slope that shifts the G foam lookup, similar
                // to a lightweight normal map without moving the water boundary.
                float4 borderAndFoam = tex2D(_WaterBorderAndNoisesMask, i.uv);
                float foamDirectionLength = max(length(_FoamDistortionDirection.xy), 0.0001);
                float2 foamDirection = _FoamDistortionDirection.xy / foamDirectionLength;
                float2 distortionUv = frac(i.uv / max(_FoamDistortionSize, 0.0001) + foamDirection * (_Time.y * _FoamDistortionSpeed));
                float foamNoise = tex2D(_WaterBorderAndNoisesMask, distortionUv).b;
                float noiseX = tex2D(_WaterBorderAndNoisesMask, distortionUv + float2(_WaterBorderAndNoisesMask_TexelSize.x, 0.0)).b;
                float noiseY = tex2D(_WaterBorderAndNoisesMask, distortionUv + float2(0.0, _WaterBorderAndNoisesMask_TexelSize.y)).b;
                float2 foamSlope = float2(foamNoise - noiseX, foamNoise - noiseY);
                float2 foamUv = saturate(i.uv + rippleOffset + foamSlope * _FoamDistortionStrength);
                float foam = tex2D(_WaterBorderAndNoisesMask, foamUv).g;
                waterColor = lerp(waterColor, _FoamColor.rgb, saturate(foam * _FoamIntensity));

                // Composite the former transparent ripple overlay directly into the water body.
                // It uses its own rotated lookup for visual variation, while the unrotated
                // height field above continues to drive refraction in the same ring timeline.
                float3 rippleNormal = normalize(float3(-ddx(rippleVisualHeight) * _RippleHeightStrength,
                                                       -ddy(rippleVisualHeight) * _RippleHeightStrength,
                                                       1.0));
                float3 rippleLightDirection = normalize(float3(_RippleLightDirection.xy, 0.8));
                float rippleLighting = saturate(dot(rippleNormal, rippleLightDirection));
                float3 rippleColor = _RippleColor.rgb * (0.35 + 0.65 * rippleLighting);
                rippleColor = lerp(rippleColor, float3(1.0, 1.0, 1.0), saturate(rippleFoam));
                float rippleAlpha = saturate(rippleVisualHeight * _RippleVisualFade * _RippleColor.a);
                waterColor = lerp(waterColor, rippleColor, rippleAlpha);

                fixed4 result = fixed4(waterColor, source.a * borderAndFoam.r * i.color.a * _Opacity);

                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif

                return result;
            }
            ENDHLSL
        }
    }
}
