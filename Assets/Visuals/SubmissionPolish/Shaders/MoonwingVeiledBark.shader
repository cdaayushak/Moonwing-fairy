Shader "Moonwing/Veiled Bark"
{
    Properties
    {
        _BaseColor("Bark",Color)=(0.24,0.20,0.29,1)
        _BaseMap("Base",2D)="white" {}
        _Emission("Emission",Float)=0
        _Wind("Wind",Float)=0
        _CameraFade("Camera clearance",Float)=1
        _Cutoff("Cutoff",Float)=0.5
        _Cull("Cull",Float)=2
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off
        UsePass "Moonwing/Clearing Botanical/MoonwingForward"
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
