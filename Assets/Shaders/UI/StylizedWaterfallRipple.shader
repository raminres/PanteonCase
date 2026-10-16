Shader "Panteon/UI/Stylized Waterfall Ripple"
{
    Properties
    {
        // A UI Image provides this texture and tint; it is used only for regular uGUI compatibility.
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Image Tint", Color) = (1, 1, 1, 1)

        // B is one authored ring. G is a stationary outer-edge fade shared by every ring.
        _RippleMask ("TX Ripples Mask (B Single Ring, G Edge Fade)", 2D) = "black" {}
        _RippleSize ("Ripple Size (X Y)", Vector) = (1, 0.70, 0, 0)
        _RippleSpeed ("Speed", Range(0.01, 1)) = 0.18
        _RippleColor ("Color", Color) = (0.78, 0.96, 1, 1)
        _Fade ("Fade", Range(0, 1)) = 0.65
        _RingSpawnScale ("Ring Spawn Scale", Range(0.02, 0.5)) = 0.10
        _RingEndScale ("Ring End Scale", Range(0.6, 1.5)) = 1.20
        _RingSpacing ("Ring Spacing", Range(0.12, 0.3)) = 0.20
        _SpawnFadeDuration ("Spawn Fade Duration", Range(0.01, 0.4)) = 0.14
        _RingEndFadeStart ("Ring End Fade Start", Range(0.4, 0.95)) = 0.70
        _FoamRingIntensity ("Ring Foam Intensity", Range(0, 3)) = 0.65
        _FoamRingRotateSpeed ("Ring Foam Rotate Speed (Degrees/Sec)", Range(-30, 30)) = 2
        _FoamRingRotationOffset ("Ring Foam Rotation Offset (Degrees)", Range(0, 180)) = 35
        _HeightStrength ("Height Effect", Range(0, 4)) = 1.25
        _LightDirection ("Highlight Direction (X Y)", Vector) = (-0.55, 0.85, 0, 0)

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
            Name "StylizedWaterfallRipple"

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
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _RippleMask;
            float4 _RippleMask_TexelSize;
            fixed4 _Color;
            fixed4 _RippleColor;
            float4 _RippleSize;
            float _RippleSpeed;
            float _Fade;
            float _RingSpawnScale;
            float _RingEndScale;
            float _RingSpacing;
            float _SpawnFadeDuration;
            float _RingEndFadeStart;
            float _FoamRingIntensity;
            float _FoamRingRotateSpeed;
            float _FoamRingRotationOffset;
            float _HeightStrength;
            float4 _LightDirection;
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

            float2 RotateAroundCentre(float2 uv, float angleRadians)
            {
                float sine = sin(angleRadians);
                float cosine = cos(angleRadians);
                float2 centredUv = uv - 0.5;
                return float2(centredUv.x * cosine - centredUv.y * sine,
                              centredUv.x * sine + centredUv.y * cosine) + 0.5;
            }

            // B contains a single ring near the edge of its UV space. R adds foam variation to
            // that same ring. Both use the same scaled, rotating lookup so they stay locked.
            float2 EvaluateRippleRing(float2 imageUv, float life, float ringIndex)
            {
                float growth = lerp(_RingSpawnScale, _RingEndScale, smoothstep(0.0, 1.0, life));
                float2 size = max(_RippleSize.xy * growth, float2(0.0001, 0.0001));
                float2 rippleUv = (imageUv - 0.5) / size + 0.5;
                float2 inBounds = step(float2(0.0, 0.0), rippleUv) * step(rippleUv, float2(1.0, 1.0));
                float spawnFade = smoothstep(0.0, _SpawnFadeDuration, life);
                // This affects only the oldest wavefront; it avoids the hard cut when a ring
                // reaches the overlay boundary while the other rings continue uninterrupted.
                float endFade = 1.0 - smoothstep(_RingEndFadeStart, 1.0, life);
                // Each staggered wavefront receives a stable angular offset, then all of them
                // rotate at the same rate. This prevents consecutive foam rings looking copied.
                float angle = radians(_Time.y * _FoamRingRotateSpeed + ringIndex * _FoamRingRotationOffset);
                float2 rotatingUv = RotateAroundCentre(rippleUv, angle);
                float4 ringMask = tex2D(_RippleMask, saturate(rotatingUv));
                float sharedFade = spawnFade * endFade * inBounds.x * inBounds.y;
                float ringHeight = ringMask.b * sharedFade;
                float ringFoam = ringMask.r * ringMask.b * sharedFade;
                return float2(ringHeight, ringFoam);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Five in-flight wavefronts form the full concentric train. New rings spawn at
                // the centre before older ones slowly leave through G's fixed edge fade.
                float life = frac(_Time.y * _RippleSpeed);
                float2 ringA = EvaluateRippleRing(i.uv, life, 0.0);
                float2 ringB = EvaluateRippleRing(i.uv, frac(life + _RingSpacing), 1.0);
                float2 ringC = EvaluateRippleRing(i.uv, frac(life + _RingSpacing * 2.0), 2.0);
                float2 ringD = EvaluateRippleRing(i.uv, frac(life + _RingSpacing * 3.0), 3.0);
                float2 ringE = EvaluateRippleRing(i.uv, frac(life + _RingSpacing * 4.0), 4.0);
                float edgeFade = tex2D(_RippleMask, i.uv).g;
                float surfaceHeight = saturate((ringA.x + ringB.x + ringC.x + ringD.x + ringE.x) * edgeFade);
                float foamDetail = saturate((ringA.y + ringB.y + ringC.y + ringD.y + ringE.y) * edgeFade) * _FoamRingIntensity;

                // Screen-space derivatives of the combined B height field form a compact normal
                // cue. This preserves the raised-water look with six texture samples per pixel.
                float3 normal = normalize(float3(-ddx(surfaceHeight) * _HeightStrength,
                                                 -ddy(surfaceHeight) * _HeightStrength,
                                                 1.0));
                float3 lightDirection = normalize(float3(_LightDirection.xy, 0.8));
                float lighting = saturate(0.45 + dot(normal, lightDirection) * 0.55);
                float alpha = saturate(surfaceHeight + foamDetail) * _Fade * i.color.a;
                fixed3 rippleColour = _RippleColor.rgb * i.color.rgb * lighting;
                fixed3 colour = lerp(rippleColour, fixed3(1.0, 1.0, 1.0), saturate(foamDetail));
                fixed4 result = fixed4(colour, alpha);

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
