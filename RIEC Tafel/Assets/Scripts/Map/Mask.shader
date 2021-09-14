Shader "Custom/Mask"
{
    SubShader
    {
        Tags { "Queue" = "Geometry+1"}

        Pass {
            Blend Zero One
        }
    }
}
