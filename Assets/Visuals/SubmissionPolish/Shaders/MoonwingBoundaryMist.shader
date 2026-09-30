Shader "Moonwing/Boundary Mist"
{
    Properties { _BaseColor("Mist",Color)=(0.42,0.49,0.66,0.20) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            struct A { float4 p:POSITION; };
            struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; };
            V vert(A a) { V o; o.world=TransformObjectToWorld(a.p.xyz); o.p=TransformWorldToHClip(o.world); return o; }
            half4 frag(V i):SV_Target
            {
                float wave=0.75+0.12*sin(i.world.x*0.22+_Time.y*0.1)+0.10*sin(i.world.z*0.33-_Time.y*0.08);
                float radius=length(i.world.xz);
                float feather=smoothstep(30,44,radius)*(1-smoothstep(64,86,radius));
                return half4(_BaseColor.rgb,_BaseColor.a*wave*feather);
            }
            ENDHLSL
        }
    }
}
