Shader "Moonwing/Enchanted Cloud Belt"
{
    Properties { _BaseColor("Pearl mist",Color)=(0.82,0.86,0.95,0.8) _Phase("Layer phase",Float)=0 }
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
            half4 _BaseColor; float _Phase;
            CBUFFER_END
            struct A { float4 p:POSITION; half4 color:COLOR; };
            struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; half alpha:TEXCOORD1; };
            V vert(A a) { V o; o.world=TransformObjectToWorld(a.p.xyz); o.p=TransformWorldToHClip(o.world); o.alpha=a.color.a; return o; }
            half4 frag(V i):SV_Target
            {
                float angle=atan2(i.world.x,i.world.z);
                float time=_Time.y*0.035;
                float cloudTop=14+3*sin(angle*5+time+_Phase)+1.7*sin(angle*13-time*1.5+_Phase);
                float body=1-smoothstep(cloudTop-4,cloudTop+6,i.world.y);
                float billow=0.5+0.5*sin(angle*11-i.world.y*0.34+time*2+_Phase);
                half3 pearl=_BaseColor.rgb*lerp(0.84,1,billow);
                float foot=smoothstep(-0.3,3.2+sin(angle*9+_Phase),i.world.y);
                // The ground itself reaches pearl mist before this bank; feather the intersection.
                return half4(pearl,saturate(_BaseColor.a*body*i.alpha*foot));
            }
            ENDHLSL
        }
    }
}
