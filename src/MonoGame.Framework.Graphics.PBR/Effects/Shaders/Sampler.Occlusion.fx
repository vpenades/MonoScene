
float GetOcclusionSample(float2 uv0, float2 uv1)
{
	float3 uvw = float3(OcclusionTextureIdx < 1 ? uv0 : uv1, 1);
	uv0.x = dot(uvw, OcclusionTransformU);
	uv0.y = dot(uvw, OcclusionTransformV);

	return SAMPLE_TEXTURE(OcclusionTexture, uv0).r;
}

