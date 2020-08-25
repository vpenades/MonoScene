
float4 GetPrimaryColor(float2 uv)
{
    float3 uv1 = float3(uv, 1);
    uv.x = dot(uv1, PrimaryTransformU);
    uv.y = dot(uv1, PrimaryTransformV);

    return sRGBToLinear(SAMPLE_TEXTURE(PrimaryTexture, uv));
}