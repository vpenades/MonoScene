
float3 getEmissiveColor(float2 uv)
{
    float3 color = EmissiveScale;

    float3 uv1 = float3(uv, 1);
    uv.x = dot(uv1, EmissiveTransformU);
    uv.y = dot(uv1, EmissiveTransformV);

    color *= sRGBToLinear(SAMPLE_TEXTURE(EmissiveTexture, uv).rgb);

    return color;
}