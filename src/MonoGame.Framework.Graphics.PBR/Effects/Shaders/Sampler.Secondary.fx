
float4 GetSecondarySample(float2 uv0, float2 uv1)
{
    float3 uvw = float3(SecondaryTextureIdx < 1 ? uv0 : uv1, 1);
    uv0.x = dot(uvw, SecondaryTransformU);
    uv0.y = dot(uvw, SecondaryTransformV);

    return SAMPLE_TEXTURE(SecondaryTexture, uv0);
}