Shader "Moonwing/Clearing Botanical"
{
    Properties
    {
        _BaseColor("Tint", Color) = (1,1,1,1)
        _Emission("Emission", Range(0,4)) = 0
        _Wind("Leaf movement", Range(0,0.2)) = 0
        _CameraFade("Near foliage clearance",Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off
        Pass
        {
            Name "MoonwingForward"
            Tags { "LightMode"="UniversalForwardOnly" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Emission;
                float _Wind;
                float _CameraFade;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; half4 color:COLOR; float fog:TEXCOORD2; half3 localLight:TEXCOORD3; UNITY_VERTEX_INPUT_INSTANCE_ID };
            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v); UNITY_TRANSFER_INSTANCE_ID(v,o);
                float3 world = TransformObjectToWorld(v.positionOS.xyz);
                world.xz += sin(_Time.y * 0.65 + world.x * 0.7 + world.z * 0.45) * _Wind * v.color.a * float2(1,0.35);
                o.positionWS=world; o.positionCS=TransformWorldToHClip(world);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.localLight=VertexLighting(world,o.normalWS);
                o.color=v.color; o.fog=ComputeFogFactor(o.positionCS.z); return o;
            }
            half4 frag(Varyings i):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                Light moon=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half3 n=normalize(i.normalWS);
                half diffuse=0.3+0.7*abs(dot(n,moon.direction));
                half3 light=half3(0.13,0.10,0.18)+SampleSH(n)*0.8+moon.color*diffuse*lerp(0.4,1,moon.shadowAttenuation)*0.7;
                light+=i.localLight;
                #if defined(_ADDITIONAL_LIGHTS)
                for(uint index=0;index<GetAdditionalLightsCount();index++)
                {
                    Light accent=GetAdditionalLight(index,i.positionWS);
                    light+=accent.color*accent.distanceAttenuation*saturate(dot(n,accent.direction));
                }
                #endif
                half3 base=i.color.rgb*_BaseColor.rgb;
                half3 col=base*(light+_Emission);
                col=lerp(col,unity_FogColor.rgb,smoothstep(30,70,length(i.positionWS.xz))*0.96);
                // Fade only geometry immediately against the lens; distant/character art is unchanged.
                float cameraDistance=distance(i.positionWS,_WorldSpaceCameraPos);
                float2 screenUV=i.positionCS.xy/_ScaledScreenParams.xy;
                float corridor=1-smoothstep(0.14,0.32,length((screenUV-float2(0.5,0.5))*float2(1,1.1)));
                float nearFade=lerp(saturate((cameraDistance-0.65)/0.6),saturate((cameraDistance-6.5)/5.5),_CameraFade*corridor);
                float dither=frac(dot(floor(i.positionCS.xy),float2(0.75487766,0.56984029)));
                clip(nearFade-dither);
                return half4(MixFog(col,i.fog),1);
            }
            ENDHLSL
        }
    }
}
