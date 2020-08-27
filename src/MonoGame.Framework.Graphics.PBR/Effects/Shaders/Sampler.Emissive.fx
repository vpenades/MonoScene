
float3 GetEmissiveSample(float2 uv0, float2 uv1)
{
    float3 uvw = float3(EmissiveTextureIdx < 1 ? uv0 : uv1, 1);
    uv0.x = dot(uvw, EmissiveTransformU);
    uv0.y = dot(uvw, EmissiveTransformV);

    return sRGBToLinear(SAMPLE_TEXTURE(EmissiveTexture, uv0).rgb);
}