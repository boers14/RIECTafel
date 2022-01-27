Shader "Custom/Mask"
{
    // Make transparent objects invisible that are in this object
    SubShader
    {
        Tags { "Queue" = "Geometry+1"}

        Pass {
            Blend Zero One
        }
    }
}
