
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// MetallicRoughness Shader
// - R : screen Red     \
// - G : screen Green    |- sRGB (*)
// - B : screen Blue    /
// - A : Alpha

// SpecularGlossiness Shader
// - R : screen Red     \
// - G : screen Green    |- sRGB (*)
// - B : screen Blue    /
// - A : Alpha

// (*) - PBR uses linear RGB for lighting calculations, and texture's
//       sRGB values need to be converted to linear RGB by applying a
//       2.2 Gamma Correction. See ToneMapping.fx

DECLARE_TEXTURE(PrimaryTexture, 1);

float4 PrimaryScale;
int PrimaryTextureIdx;
float3 PrimaryTransformU;
float3 PrimaryTransformV;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4 GetPrimarySample(float2 uv0, float2 uv1)
{
    float3 uvw = float3(PrimaryTextureIdx < 1 ? uv0 : uv1, 1);
    uv0.x = dot(uvw, PrimaryTransformU);
    uv0.y = dot(uvw, PrimaryTransformV);

    return SAMPLE_TEXTURE(PrimaryTexture, uv0);
}