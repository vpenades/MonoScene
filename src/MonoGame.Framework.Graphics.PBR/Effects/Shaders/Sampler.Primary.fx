
float4 GetPrimaryColor(float2 uv)
{
    return sRGBToLinear(SAMPLE_TEXTURE(PrimaryTexture, uv));
}