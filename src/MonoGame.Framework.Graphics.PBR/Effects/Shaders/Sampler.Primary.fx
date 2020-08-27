
float4 GetPrimaryColor(float2 uv0, float2 uv1)
{
    float3 uvx = float3(PrimaryTextureIdx < 1 ? uv0 : uv1, 1);
    uv0.x = dot(uvx, PrimaryTransformU);
    uv0.y = dot(uvx, PrimaryTransformV);

    return sRGBToLinear(SAMPLE_TEXTURE(PrimaryTexture, uv0));
}