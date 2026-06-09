Shader "UI/ImageOutline12"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.2)) = 0.05
        
        // 下面这些是 UI 必需的 Stencil 属性，保证遮罩等正常
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        ColorMask [_ColorMask]
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Color;
            float4 _OutlineColor;
            float _OutlineWidth;
            float4 _ClipRect;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // 采样周围像素的 alpha
                float outline = 0;
                float w = _MainTex_TexelSize.x * _OutlineWidth;
                float h = _MainTex_TexelSize.y * _OutlineWidth;
                
                outline += tex2D(_MainTex, i.uv + float2(-w, -h)).a;
                outline += tex2D(_MainTex, i.uv + float2(0, -h)).a;
                outline += tex2D(_MainTex, i.uv + float2(w, -h)).a;
                outline += tex2D(_MainTex, i.uv + float2(-w, 0)).a;
                outline += tex2D(_MainTex, i.uv + float2(w, 0)).a;
                outline += tex2D(_MainTex, i.uv + float2(-w, h)).a;
                outline += tex2D(_MainTex, i.uv + float2(0, h)).a;
                outline += tex2D(_MainTex, i.uv + float2(w, h)).a;
                
                outline = saturate(outline);
                
                float4 texColor = tex2D(_MainTex, i.uv) * i.color;
                float edge = outline * (1 - texColor.a);
                texColor.rgb = lerp(texColor.rgb, _OutlineColor.rgb, edge * _OutlineColor.a);
                texColor.a = max(texColor.a, edge * _OutlineColor.a);
                
                // 支持 UI Mask 裁剪
                #ifdef UNITY_UI_CLIP_RECT
                texColor.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif
                
                return texColor;
            }
            ENDCG
        }
    }
}