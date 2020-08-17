
float getAmbientOcclusion(float2 uv)
{
    return SAMPLE_TEXTURE(OcclusionTexture, uv).r;
}