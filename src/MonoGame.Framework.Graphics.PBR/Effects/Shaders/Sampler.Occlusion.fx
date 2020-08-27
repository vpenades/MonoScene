
float GetAmbientOcclusion(float2 uv0, float2 uv1)
{
	float3 uvx = float3(OcclusionTextureIdx < 1 ? uv0 : uv1, 1);
	uv0.x = dot(uvx, OcclusionTransformU);
	uv0.y = dot(uvx, OcclusionTransformV);

	return SAMPLE_TEXTURE(OcclusionTexture, uv0).r;
}

