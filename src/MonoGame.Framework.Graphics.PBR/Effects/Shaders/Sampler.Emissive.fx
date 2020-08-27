
float3 GetEmissiveColor(float2 uv0, float2 uv1)
{
    float3 color = EmissiveScale;

    float3 uvx = float3(EmissiveTextureIdx < 1 ? uv0 : uv1, 1);
    uv0.x = dot(uvx, EmissiveTransformU);
    uv0.y = dot(uvx, EmissiveTransformV);

    color *= sRGBToLinear(SAMPLE_TEXTURE(EmissiveTexture, uv0).rgb);

    return color;
}