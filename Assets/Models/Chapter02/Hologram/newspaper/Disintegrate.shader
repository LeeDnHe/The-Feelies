Shader "Custom/Hologram_Edge_Fix"
{
    Properties
    {
        _MainTex ("Hologram PNG", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        [HDR] _HoloColor ("Hologram Tint Color", Color) = (0, 0.5, 1, 1)
        _DissolveAmount ("Dissolve Amount", Range(0, 1.2)) = 0
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.02 // 기본값을 낮게 설정
        [HDR] _EdgeColor ("Edge Color", Color) = (0, 0.8, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _HoloColor;
            float _DissolveAmount;
            float _EdgeWidth;
            float4 _EdgeColor;

            Varyings vert (Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target {
                half4 col = tex2D(_MainTex, IN.uv);
                float noise = tex2D(_NoiseTex, IN.uv).r;
                float brightness = max(col.r, max(col.g, col.b));

                // 1. [순서 변경] 먼저 버릴 부분을 확실히 버립니다.
                // 노이즈가 진행도보다 낮으면 아예 연산 종료 (discard)
                if (brightness < 0.05 || noise < _DissolveAmount) {
                    discard;
                }

                // 2. 남은 부분에서만 Edge를 계산합니다. 
                // 이제 이미 지워진 구석탱이에 하얀 잔상이 남을 수 없습니다.
                float edgeLogic = 0;
                if (_EdgeWidth > 0) {
                    // 노이즈가 (진행도 + 두께)보다 작을 때만 빛나게 설정
                    edgeLogic = step(noise, _DissolveAmount + _EdgeWidth);
                }

                half3 finalColor = col.rgb * _HoloColor.rgb;
                finalColor += edgeLogic * _EdgeColor.rgb;

                return half4(finalColor, brightness * _HoloColor.a);
            }
            ENDHLSL
        }
    }
}