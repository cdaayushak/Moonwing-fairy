Shader "Moonwing/Moonlit Sky"
{
    Properties { _MoonDirection("Moon direction",Vector)=(0.04,-0.02,1,0) }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _MoonDirection;
            CBUFFER_END
            struct A { float4 p:POSITION; };
            struct V { float4 p:SV_POSITION; float3 dir:TEXCOORD0; };
            V vert(A a) { V o; o.p=TransformObjectToHClip(a.p.xyz); o.dir=a.p.xyz; return o; }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            half4 frag(V i):SV_Target
            {
                float3 d=normalize(i.dir);
                half3 col=lerp(half3(0.065,0.085,0.16),half3(0.008,0.014,0.048),pow(saturate(d.y*1.5),0.45));
                col=lerp(unity_FogColor.rgb,col,smoothstep(-0.035,0.16,d.y));
                float distanceToMoon=length(d-normalize(_MoonDirection.xyz));
                col+=half3(0.16,0.22,0.38)*exp(-distanceToMoon*16);
                float disc=1-smoothstep(0.022,0.0235,distanceToMoon);
                float detail=0.86+0.1*sin(d.x*420)*sin(d.y*320)+0.04*sin(d.z*930);
                col+=half3(2.4,2.65,3)*disc*detail;
                float2 uv=float2(atan2(d.x,d.z),asin(d.y))*115;
                float2 cell=floor(uv);
                float seed=hash(cell);
                float2 offset=float2(hash(cell+13.7),hash(cell+51.3))*0.6+0.2;
                float radius=lerp(0.055,0.16,hash(cell+21));
                float aa=max(fwidth(uv.x),fwidth(uv.y))*0.65;
                float star=step(0.975,seed)*(1-smoothstep(radius,radius+aa,length(frac(uv)-offset)));
                // Independent phases/frequencies: gentle variation, never a synchronous sky flash.
                float twinkle=0.72+0.28*sin(_Time.y*lerp(0.45,1.3,hash(cell+9))+seed*61.8);
                col+=star*twinkle*lerp(half3(1.2,1.6,2.1),half3(1.9,1.5,2.2),seed)*smoothstep(-0.075,0.025,d.y);
                return half4(col,1);
            }
            ENDHLSL
        }
    }
}
