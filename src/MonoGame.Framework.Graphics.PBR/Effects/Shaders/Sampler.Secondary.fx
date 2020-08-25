
float4 GetSecondaryColor(float2 uv)
{
    float3 uv1 = float3(uv, 1);
    uv.x = dot(uv1, SecondaryTransformU);
    uv.y = dot(uv1, SecondaryTransformV);

    return SAMPLE_TEXTURE(SecondaryTexture, uv);
}