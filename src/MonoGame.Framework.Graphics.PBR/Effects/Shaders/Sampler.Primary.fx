
float4 GetPrimarySample(float2 uv0, float2 uv1)
{
    float3 uvw = float3(PrimaryTextureIdx < 1 ? uv0 : uv1, 1);
    uv0.x = dot(uvw, PrimaryTransformU);
    uv0.y = dot(uvw, PrimaryTransformV);

    return sRGBToLinear(SAMPLE_TEXTURE(PrimaryTexture, uv0));
}