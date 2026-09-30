Shader "Moonwing/Clearing Ground"
{
    Properties { _BaseColor("Moss",Color)=(0.045,0.095,0.115,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="UniversalForwardOnly" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END
            struct A { float4 p:POSITION; };
            struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; float fog:TEXCOORD1; };
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float noise(float2 p) { float2 a=floor(p), f=frac(p); f=f*f*(3-2*f); return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y); }
            V vert(A a) { V o; o.world=TransformObjectToWorld(a.p.xyz); o.p=TransformWorldToHClip(o.world); o.fog=ComputeFogFactor(o.p.z); return o; }
            half4 frag(V i):SV_Target
            {
                float2 p=i.world.xz;
                float n=noise(p*0.7)*0.5+noise(p*2.3)*0.3+noise(p*12)*0.2;
                float path=1-smoothstep(1.8,4.6,abs(p.x-sin(p.y*0.17)*2.4)+noise(p*0.6)*1.2);
                half3 moss=_BaseColor.rgb*lerp(0.55,1.65,n);
                half3 soil=half3(0.115,0.12,0.18)*lerp(0.7,1.15,n);
                half3 color=lerp(moss,soil,path*0.7);
                Light moon=GetMainLight(TransformWorldToShadowCoord(i.world));
                color*=half3(0.08,0.09,0.15)+SampleSH(half3(0,1,0))+moon.color*(0.3+0.7*saturate(moon.direction.y))*lerp(0.35,1,moon.shadowAttenuation);
                color=lerp(color,unity_FogColor.rgb,smoothstep(30,70,length(i.world.xz))*0.96);
                // Per-pixel fog is essential on the large extension: interpolating corner fog
                // would fully fog the whole quad and expose a seam around the original floor.
                half3 fogged=MixFog(color,ComputeFogFactor(TransformWorldToHClip(i.world).z));
                fogged=lerp(fogged,half3(0.75,0.80,0.89),smoothstep(34,70,length(i.world.xz)));
                return half4(fogged,1);
            }
            ENDHLSL
        }
    }
}
