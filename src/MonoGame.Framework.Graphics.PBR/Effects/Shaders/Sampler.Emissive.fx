
float3 getEmissiveColor(float2 uv)
{
    float3 color = EmissiveScale;

    color *= sRGBToLinear(SAMPLE_TEXTURE(EmissiveTexture, uv)).rgb;

    return color;
}