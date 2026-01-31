Shader "Custom/AfterImageBlur"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Blur ("Blur", Range(0, 0.01)) = 0.003
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Color;
            float _Blur;

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 Sample(float2 uv)
            {
                return tex2D(_MainTex, uv);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 off = float2(_Blur, _Blur);

                // 9-tap blur (box-ish but looks soft enough for ghosts)
                fixed4 c = 0;
                c += Sample(i.uv + float2(-off.x, -off.y));
                c += Sample(i.uv + float2( 0,     -off.y));
                c += Sample(i.uv + float2( off.x, -off.y));
                c += Sample(i.uv + float2(-off.x,  0));
                c += Sample(i.uv);
                c += Sample(i.uv + float2( off.x,  0));
                c += Sample(i.uv + float2(-off.x,  off.y));
                c += Sample(i.uv + float2( 0,      off.y));
                c += Sample(i.uv + float2( off.x,  off.y));
                c /= 9.0;

                c *= i.color;
                return c;
            }
            ENDCG
        }
    }
}
