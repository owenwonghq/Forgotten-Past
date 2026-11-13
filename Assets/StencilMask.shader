Shader "Custom/StencilMask"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry-1" "RenderType" = "Opaque" }
        Pass
        {
            ColorMask 0
            ZWrite Off
        }
    }
}