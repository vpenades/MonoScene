
float getAmbientOcclusion(float2 uv)
{
	float3 uv1 = float3(uv, 1);
	uv.x = dot(uv1, OcclusionTransformU);
	uv.y = dot(uv1, OcclusionTransformV);

	return SAMPLE_TEXTURE(OcclusionTexture, uv).r;
}