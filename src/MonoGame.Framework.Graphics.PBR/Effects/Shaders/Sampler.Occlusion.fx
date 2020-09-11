
// ========================================= CONSTANTS =========================================

// All Shaders
// - R : Oclussion, or unused if separated oclussion map is defined (*)
// - G : Unused
// - B : Unused
// - A : Unused

// (*) - Occlusion channel intentionally uses the R channel so it overlaps with
//       the R channel of MetallicRoughness Secondary Texture.
//       It might feel like having a separate texture for occlusion is a waste,
//       since it can be packed with the metallic roughness. But there's two cases
//       that require a separarate occlusion channel.
//       - It cannot be packed with SpecularGlossiness, which uses all RGBA channels.
//       - When occlusion map uses the second set of UV coordinates, or when
//         MetallicRoughness and Occlusion need different texture sizes.

DECLARE_TEXTURE(OcclusionTexture, 4);

float OcclusionScale;
int OcclusionTextureIdx;
float3 OcclusionTransformU;
float3 OcclusionTransformV;

// ========================================= FUNCTIONS =========================================

float GetOcclusionSample(float2 uv0, float2 uv1)
{
	float3 uvw = float3(OcclusionTextureIdx < 1 ? uv0 : uv1, 1);
	uv0.x = dot(uvw, OcclusionTransformU);
	uv0.y = dot(uvw, OcclusionTransformV);

	return SAMPLE_TEXTURE(OcclusionTexture, uv0).r;
}

