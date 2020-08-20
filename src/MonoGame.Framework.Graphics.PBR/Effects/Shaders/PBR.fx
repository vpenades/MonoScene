//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Shader globals
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// https://github.com/KhronosGroup/glTF-Sample-Viewer/tree/master/src/shaders

#include "MacrosSM4.fxh"

#include "Functions.fx"
#include "BDRF.fx"
#include "Punctual.fx"

#define SKINNED_EFFECT_MAX_BONES   128

DECLARE_TEXTURE(NormalTexture, 0);

DECLARE_TEXTURE(PrimaryTexture, 1);     // either BaseColor or Diffuse
DECLARE_TEXTURE(SecondaryTexture, 2);   // either MetallicRoughness or SpecularGlossiness

DECLARE_TEXTURE(EmissiveTexture, 3);
DECLARE_TEXTURE(OcclusionTexture, 4);

BEGIN_CONSTANTS

    float4x4 World;
    float4x4 View;
    float4x4 Projection;
    float4x3 Bones[SKINNED_EFFECT_MAX_BONES]; // 4x3 is enough, and saves constants    

    float3 CameraPosition;

    float2 AlphaTransform;
    float AlphaCutoff;

    float Exposure; // parameter for ToneMapping.toneMap

    int NumberOfLights;
    float4 LightParam0[3];
    float4 LightParam1[3];
    float4 LightParam2[3];
    float4 LightParam3[3];

    // Metallic Roughness Material.

    float NormalScale;

    float4 PrimaryScale;    // either BaseColor or Diffuse
    float4 SecondaryScale;  // either MetallicRoughness or SpecularGlossiness

    float OcclusionScale;

    float3 EmissiveScale;    

END_CONSTANTS


Light getLight(int index)
{
    Light l;
    l.direction = LightParam0[index].xyz;
    l.range = LightParam0[index].w;

    l.color = LightParam1[index].xyz;
    l.intensity = LightParam1[index].w;

    l.position = LightParam2[index].xyz;
    l.innerConeCos = LightParam2[index].w;

    l.outerConeCos = LightParam3[index].x;
    l.type = (int)LightParam3[index].y;

    l.padding;

    return l;
}

#include "ToneMapping.fx"

// #include "IBL.fx"


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// PIXEL SHADERS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// Vertex Shader output, Pixel Shader input
struct VsOutTexNorm
{
    float4 PositionPS : SV_Position;

    float4 Color: COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 PositionWS : TEXCOORD1;

    // float3x3 TangentBasis : TBASIS; requires Shader Model 4 :(

    float3 TangentBasisX : TEXCOORD2;
    float3 TangentBasisY : TEXCOORD3;
    float3 TangentBasisZ : TEXCOORD4;
};


#include "PBR.Pixel.fx"

#include "Sampler.Primary.fx"
#include "Sampler.Emissive.fx"
#include "Sampler.Occlusion.fx"

float4 PsShader(VsOutTexNorm input, bool hasPerturbedNormals, bool hasPrimary, bool hasSecondary, bool hasEmissive, bool hasOcclusion)
{
    // get primary color

    float4 f_primary = PrimaryScale * input.Color;
    if (hasPrimary) f_primary *= GetPrimaryColor(input.TextureCoordinate);

    // alpha cutoff
    clip((f_primary.a < AlphaCutoff) ? -1 : 1);

    // alpha blend
    f_primary.a = mad(f_primary.a, AlphaTransform.x, AlphaTransform.y);        

    // normals

    NormalInfo ninfo;

    if (hasPerturbedNormals)
    {
        ninfo = getNormalInfo(input);
    }
    else
    {        
        ninfo.ng = normalize(input.TangentBasisZ);
        ninfo.n = ninfo.ng;
        ninfo.t = 0; // should generate some random T & b ?
        ninfo.b = 0;        
    }
    

    float4 f_secondary = 1;
    if (hasSecondary) f_secondary *= SAMPLE_TEXTURE(SecondaryTexture, input.TextureCoordinate);

    float3 f_emissive = EmissiveScale;
    if (hasEmissive) f_emissive *= getEmissiveColor(input.TextureCoordinate);

#ifdef MATERIAL_METALLICROUGHNESS
    float f_occlusion = f_secondary.r;
#else
    float f_occlusion = 1; // we could use f_primary.a if it's opaque
#endif
    if (hasOcclusion) f_occlusion = getAmbientOcclusion(input.TextureCoordinate);

    float3 color = PsWithPBR(input.PositionWS, ninfo, f_primary.rgb, f_secondary);    

    color += f_emissive;

    color = lerp(color, color * f_occlusion, OcclusionScale);

    color = toneMap(color);    

    return float4(color.xyz, f_primary.a);
}