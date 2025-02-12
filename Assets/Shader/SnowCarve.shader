Shader "Custom/SnowCarve" {
    Properties {
        _MainTex("Main Texture", 2D) = "white" {}
        _MaskTex("Mask Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                // 마스크의 red 채널 값에 따라 알파 조절 (1이면 불투명, 0이면 투명)
                fixed mask = tex2D(_MaskTex, i.uv).r;
                col.a *= mask;
                return col;
            }
            ENDCG
        }
    }
}
