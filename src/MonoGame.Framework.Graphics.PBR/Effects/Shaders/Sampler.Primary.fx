
float4 getBaseColor(float2 uv, float4 vertexColor)
{
    float4 baseColor = PrimaryScale;

    baseColor *= sRGBToLinear(SAMPLE_TEXTURE(PrimaryTexture, uv));

    return baseColor * vertexColor;
}