Shader "Moonwing/Moonlight Ribbon"
{
    Properties { _Intensity("Glow",Range(0,5))=2 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float _Intensity;
            CBUFFER_END
            struct A { float4 p:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct V { float4 p:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            V vert(A a) { V o; o.p=TransformObjectToHClip(a.p.xyz); o.uv=a.uv; o.color=a.color; return o; }
            half4 frag(V i):SV_Target { float edge=pow(saturate(1-abs(i.uv.y*2-1)),1.5); return half4(i.color.rgb*_Intensity,i.color.a*edge); }
            ENDHLSL
        }
    }
}
