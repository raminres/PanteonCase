Shader "Panteon/UI/Stylized Leaf Wind"
{
    Properties
    {
        // Standard uGUI properties. The source sprite supplies the leaf silhouette.
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Image Tint", Color) = (1, 1, 1, 1)

        _WindDirection ("Wind Direction (X Y)", Vector) = (1, 0, 0, 0)
        _WindStrength ("Body Bend Strength", Range(0, 0.12)) = 0.025
        _WindSpeed ("Wind Speed", Range(0, 3)) = 0.65
        _WaveFrequency ("Broad Wave Shape", Range(0.1, 2)) = 0.55
        _AnchorFalloff ("Base Anchor Falloff", Range(0.5, 4)) = 1.6

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
            Name "StylizedLeafWind"

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
                float4 localUvAndPivot : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 localUvAndPivot : TEXCOORD1;
                float4 worldPosition : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _WindDirection;
            float _WindStrength;
            float _WindSpeed;
            float _WaveFrequency;
            float _AnchorFalloff;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // The segmented LeafWindImage mesh turns this coherent sine into a true
                // left-right deformation of the leaf silhouette rather than UV distortion.
                float pivotToTipHeight = max(1.0 - v.localUvAndPivot.z, 0.0001);
                float anchoredGradient = saturate((v.localUvAndPivot.y - v.localUvAndPivot.z) / pivotToTipHeight);
                float anchorWeight = pow(anchoredGradient, _AnchorFalloff);
                float directionLength = max(length(_WindDirection.xy), 0.0001);
                float2 direction = _WindDirection.xy / directionLength;
                float3 objectOrigin = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)).xyz;
                float phase = (_Time.y * _WindSpeed) + dot(objectOrigin.xy, float2(0.071, 0.113));
                float sideToSideSway = sin(phase);
                float broadWaveShape = 0.85 + (sin((anchoredGradient - 0.5) * _WaveFrequency * 3.14159265) * 0.15);
                float vertexOffset = sideToSideSway * broadWaveShape * anchorWeight *
                                     _WindStrength * v.localUvAndPivot.w;
                v.vertex.xy += direction * vertexOffset;

                o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.localUvAndPivot = v.localUvAndPivot;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the original UVs. All visible motion comes from vertex displacement,
                // eliminating the small travelling texture-noise pattern of the previous pass.
                fixed4 leafColor = tex2D(_MainTex, i.uv) * i.color;

                #ifdef UNITY_UI_CLIP_RECT
                leafColor.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(leafColor.a - 0.001);
                #endif

                return leafColor;
            }
            ENDHLSL
        }
    }
}
