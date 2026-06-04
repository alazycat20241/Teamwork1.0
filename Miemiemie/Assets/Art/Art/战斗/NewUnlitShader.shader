Shader "Custom/BackgroundMultiply"
{
    Properties
    {
        _MainTex ("Background Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _Intensity ("Overlay Intensity", Range(0,1)) = 1.0
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            float4 _MainTex_ST;
            float4 _OverlayTex_ST;
            fixed4 _Color;
            float _Intensity;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 采样背景图
                fixed4 bg = tex2D(_MainTex, i.uv);
                // 采样纹理图
                fixed4 overlay = tex2D(_OverlayTex, i.uv);
                
                // 正片叠底效果：将纹理的暗部作用在背景上
                // 公式解释：bg * (lerp(1, overlay, _Intensity))
                // 当overlay为白色(1)时，背景不变；为深色时，背景变暗
                fixed3 darkened = bg.rgb * lerp(fixed3(1,1,1), overlay.rgb, _Intensity);
                
                // 强制背景完全不透明
                fixed4 result = fixed4(darkened, 1.0);
                result *= _Color;
                return result;
            }
            ENDCG
        }
    }
}