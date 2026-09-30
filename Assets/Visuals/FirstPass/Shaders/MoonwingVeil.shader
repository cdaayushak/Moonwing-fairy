Shader "Moonwing/Silken Veil"
{
    Properties { _BaseColor("Tint",Color)=(1,1,1,0.4) _Emission("Glow",Range(0,5))=1 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Emission;
            CBUFFER_END
            struct A { float4 p:POSITION; half4 color:COLOR; };
            struct V { float4 p:SV_POSITION; half4 color:COLOR; float fog:TEXCOORD0; };
            V vert(A a) { V o; o.p=TransformObjectToHClip(a.p.xyz); o.color=a.color; o.fog=ComputeFogFactor(o.p.z); return o; }
            half4 frag(V i):SV_Target { half4 c=i.color*_BaseColor; return half4(MixFog(c.rgb*_Emission,i.fog),c.a); }
            ENDHLSL
        }
    }
}
