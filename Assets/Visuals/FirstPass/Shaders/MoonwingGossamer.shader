Shader "Moonwing/Gossamer"
{
    Properties { _BaseColor("Tint and opacity",Color)=(0.8,0.9,1,0.42) _Emission("Gentle radiance",Range(0,2))=0.7 }
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
                half4 _BaseColor; float _Emission;
            CBUFFER_END
            struct A { float4 p:POSITION; float3 normal:NORMAL; half4 color:COLOR; };
            struct V { float4 p:SV_POSITION; half4 color:COLOR; float3 world:TEXCOORD0; half3 normal:TEXCOORD1; float fog:TEXCOORD2; };
            V vert(A a) { V o; o.world=TransformObjectToWorld(a.p.xyz); o.p=TransformWorldToHClip(o.world); o.normal=TransformObjectToWorldNormal(a.normal); o.color=a.color; o.fog=ComputeFogFactor(o.p.z); return o; }
            half4 frag(V i):SV_Target
            {
                half angle=1-abs(dot(normalize(i.normal),normalize(GetWorldSpaceViewDir(i.world))));
                half shimmer=0.5+0.5*sin(_Time.y*0.65+i.world.y*2+i.world.x);
                half3 iridescence=lerp(half3(0.66,0.85,0.9),half3(0.84,0.73,0.91),saturate(angle*0.7+shimmer*0.3));
                return half4(MixFog(i.color.rgb*_BaseColor.rgb*iridescence*_Emission,i.fog),i.color.a*_BaseColor.a*(0.85+0.15*shimmer));
            }
            ENDHLSL
        }
    }
}
