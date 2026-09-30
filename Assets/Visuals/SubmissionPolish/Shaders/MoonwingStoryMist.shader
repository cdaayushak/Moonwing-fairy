Shader "Moonwing/Story Mist"
{
    Properties { _MainTex("Texture",2D)="white" {} _Clock("Unscaled drift",Float)=0 _Clear("Clearing",Range(0,1))=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off ZTest Always Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float _Clock, _Clear;
            struct A { float4 p:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct V { float4 p:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            V vert(A a) { V o; o.p=TransformObjectToHClip(a.p.xyz); o.uv=a.uv; o.color=a.color; return o; }
            half4 frag(V i):SV_Target
            {
                float2 p=i.uv; p.y-=_Clear*0.15;
                float wave=sin(p.x*12+_Clock*0.12)*0.024+sin(p.x*27-_Clock*0.18)*0.014;
                float back=1-smoothstep(0.17+wave,0.34+wave,p.y);
                float middle=1-smoothstep(0.10-wave,0.25-wave,p.y);
                float front=1-smoothstep(0.04+wave*1.5,0.17+wave*1.5,p.y);
                float alpha=1-(1-back*0.58)*(1-middle*0.43)*(1-front*0.36);
                float roll=0.5+0.5*sin(p.x*9-p.y*17+_Clock*0.1);
                half3 color=lerp(half3(0.73,0.80,0.91),half3(0.92,0.90,0.97),roll);
                return half4(color,alpha*(1-_Clear)*i.color.a);
            }
            ENDHLSL
        }
    }
}
