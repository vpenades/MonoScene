
float4 GetSecondaryColor(float2 uv0, float2 uv1)
{
    float3 uvx = float3(SecondaryTextureIdx < 1 ? uv0 : uv1, 1);
    uv0.x = dot(uvx, SecondaryTransformU);
    uv0.y = dot(uvx, SecondaryTransformV);

    return SAMPLE_TEXTURE(SecondaryTexture, uv0);
}

float GetSecondaryOcclusion(float2 uv0, float2 uv1)
{
	float3 uvx = float3(OcclusionTextureIdx < 1 ? uv0 : uv1, 1);
	uv0.x = dot(uvx, OcclusionTransformU);
	uv0.y = dot(uvx, OcclusionTransformV);

	return SAMPLE_TEXTURE(SecondaryTexture, uv0).r;
}