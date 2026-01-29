Shader "Hidden/BlobTracking"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        
        ZWrite Off
        Cull Off
        ZTest Always
        
        Pass
        {
            Name "BlobDetection"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragBlob
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            // Blob Tracking パラメータ
            float _Threshold;
            float _Smoothness;
            float _BlurRadius;
            float _Intensity;
            float4 _BlobColor;
            float4 _BackgroundColor;
            int _Iterations;
            float _EdgeWidth;
            float _Contrast;
            float _InvertBlob;
            float _ShowEdges;
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            
            // 輝度計算
            float GetLuminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }
            
            // ガウシアンブラー
            float3 GaussianBlur(float2 uv, float radius)
            {
                float3 color = float3(0, 0, 0);
                float2 texelSize = _MainTex_TexelSize.xy * radius;
                
                float weights[9] = {
                    0.0625, 0.125, 0.0625,
                    0.125,  0.25,  0.125,
                    0.0625, 0.125, 0.0625
                };
                
                float2 offsets[9] = {
                    float2(-1, -1), float2(0, -1), float2(1, -1),
                    float2(-1,  0), float2(0,  0), float2(1,  0),
                    float2(-1,  1), float2(0,  1), float2(1,  1)
                };
                
                [unroll]
                for (int i = 0; i < 9; i++)
                {
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offsets[i] * texelSize).rgb * weights[i];
                }
                
                return color;
            }
            
            // シンプルブラー（高速版）
            float3 SimpleBlur(float2 uv, float radius)
            {
                float3 color = float3(0, 0, 0);
                float2 texelSize = _MainTex_TexelSize.xy * radius;
                float total = 0;
                
                [unroll]
                for (int x = -2; x <= 2; x++)
                {
                    [unroll]
                    for (int y = -2; y <= 2; y++)
                    {
                        float weight = 1.0 / (1.0 + abs(x) + abs(y));
                        color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(x, y) * texelSize).rgb * weight;
                        total += weight;
                    }
                }
                
                return color / total;
            }
            
            float4 FragBlob(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                
                // オリジナル画像
                float3 original = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
                
                // ブラーをかけた画像
                float3 blurred = float3(0, 0, 0);
                float2 texelSize = _BlitTexture_TexelSize.xy * _BlurRadius;
                float total = 0;
                
                // 可変ブラー（iterations回）
                int blurSamples = max(1, _Iterations);
                for (int i = 0; i < blurSamples; i++)
                {
                    float angle = (float)i / (float)blurSamples * 6.283185;
                    float2 offset = float2(cos(angle), sin(angle)) * texelSize;
                    blurred += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset).rgb;
                    total += 1.0;
                }
                blurred = (blurred + original) / (total + 1.0);
                
                // 輝度を計算
                float lum = GetLuminance(blurred);
                
                // コントラスト調整
                lum = saturate((lum - 0.5) * _Contrast + 0.5);
                
                // しきい値処理（スムーズステップ）
                float blob = smoothstep(_Threshold - _Smoothness, _Threshold + _Smoothness, lum);
                
                // 反転オプション
                blob = lerp(blob, 1.0 - blob, _InvertBlob);
                
                // エッジ検出（シンプル版）
                float edge = 0;
                if (_ShowEdges > 0.5)
                {
                    float2 edgeTexel = _BlitTexture_TexelSize.xy * _EdgeWidth;
                    float lumL = GetLuminance(SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - float2(edgeTexel.x, 0)).rgb);
                    float lumR = GetLuminance(SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(edgeTexel.x, 0)).rgb);
                    float lumU = GetLuminance(SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv - float2(0, edgeTexel.y)).rgb);
                    float lumD = GetLuminance(SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + float2(0, edgeTexel.y)).rgb);
                    
                    // コントラスト適用
                    lumL = saturate((lumL - 0.5) * _Contrast + 0.5);
                    lumR = saturate((lumR - 0.5) * _Contrast + 0.5);
                    lumU = saturate((lumU - 0.5) * _Contrast + 0.5);
                    lumD = saturate((lumD - 0.5) * _Contrast + 0.5);
                    
                    float blobL = smoothstep(_Threshold - _Smoothness, _Threshold + _Smoothness, lumL);
                    float blobR = smoothstep(_Threshold - _Smoothness, _Threshold + _Smoothness, lumR);
                    float blobU = smoothstep(_Threshold - _Smoothness, _Threshold + _Smoothness, lumU);
                    float blobD = smoothstep(_Threshold - _Smoothness, _Threshold + _Smoothness, lumD);
                    
                    edge = abs(blobL - blobR) + abs(blobU - blobD);
                    edge = saturate(edge * 2);
                }
                
                // 最終色の計算
                float3 blobColorResult = lerp(_BackgroundColor.rgb, _BlobColor.rgb, blob);
                
                // エッジをハイライト
                blobColorResult = lerp(blobColorResult, float3(1, 1, 1), edge * _ShowEdges);
                
                // オリジナルとブレンド
                float3 finalColor = lerp(original, blobColorResult, _Intensity);
                
                return float4(finalColor, 1);
            }
            
            ENDHLSL
        }
    }
}
