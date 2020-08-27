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


    
    float NormalScale;
    int NormalTextureIdx;
    float3 NormalTransformU;
    float3 NormalTransformV;

    // either BaseColor or Diffuse
    float4 PrimaryScale;
    int PrimaryTextureIdx;
    float3 PrimaryTransformU;
    float3 PrimaryTransformV;

    // either MetallicRoughness or SpecularGlossiness
    float4 SecondaryScale;
    int SecondaryTextureIdx;
    float3 SecondaryTransformU;
    float3 SecondaryTransformV;

    float OcclusionScale;
    int OcclusionTextureIdx;
    float3 OcclusionTransformU;
    float3 OcclusionTransformV;

    float3 EmissiveScale;
    int EmissiveTextureIdx;
    float3 EmissiveTransformU;
    float3 EmissiveTransformV;

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
    float2 TextureCoordinate0 : TEXCOORD0;
    float2 TextureCoordinate1 : TEXCOORD1;
    float3 PositionWS : TEXCOORD2;

    // float3x3 TangentBasis : TBASIS; requires Shader Model 4 :(

    float3 TangentBasisX : TEXCOORD3;
    float3 TangentBasisY : TEXCOORD4;
    float3 TangentBasisZ : TEXCOORD5;
};


#include "PBR.Pixel.fx"

#include "Sampler.Normal.fx"
#include "Sampler.Primary.fx"
#include "Sampler.Secondary.fx"
#include "Sampler.Emissive.fx"
#include "Sampler.Occlusion.fx"

float4 PsShader(VsOutTexNorm input, bool hasPerturbedNormals, bool hasPrimary, bool hasSecondary, bool hasEmissive, bool hasOcclusionMap)
{
    // get primary color

    float4 f_primary = PrimaryScale * input.Color;
    if (hasPrimary) f_primary *= GetPrimarySample(input.TextureCoordinate0, input.TextureCoordinate1);

    // alpha cutoff
    clip((f_primary.a < AlphaCutoff) ? -1 : 1);

    // alpha blend
    f_primary.a = mad(f_primary.a, AlphaTransform.x, AlphaTransform.y);        

    // normals

    NormalInfo ninfo;

    if (hasPerturbedNormals)
    {
        ninfo = GetNormalSample(input);
    }
    else
    {        
        ninfo.ng = normalize(input.TangentBasisZ);
        ninfo.n = ninfo.ng;
        ninfo.t = 0; // should generate some random T & b ?
        ninfo.b = 0;        
    }

    // get additional textures    

    float4 f_secondary = 1;
    if (hasSecondary) f_secondary = GetSecondarySample(input.TextureCoordinate0, input.TextureCoordinate1);    

    float3 color = PsWithPBR(input.PositionWS, ninfo, f_primary.rgb, f_secondary);    

    float3 f_emissive = EmissiveScale;
    if (hasEmissive) f_emissive *= GetEmissiveSample(input.TextureCoordinate0, input.TextureCoordinate1);

    color += f_emissive;
    
    if (hasOcclusionMap)
    {
        float f_occlusion = GetOcclusionSample(input.TextureCoordinate0, input.TextureCoordinate1);
        color = lerp(color, color * f_occlusion, OcclusionScale);
    }    

    color = toneMap(color);    

    return float4(color.xyz, f_primary.a);
}