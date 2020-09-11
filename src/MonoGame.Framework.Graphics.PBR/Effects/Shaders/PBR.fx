//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Shader globals
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// https://github.com/KhronosGroup/glTF-Sample-Viewer/tree/master/src/shaders

#include "MacrosSM4.fxh"

#include "Functions.fx"
#include "BDRF.fx"
#include "ToneMapping.fx"

float3 CameraPosition;    

float2 AlphaTransform;
float AlphaCutoff;
    
float3 AmbientLight;

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

    // float3x3 TangentBasis : TBASIS; // requires Shader Model 4 :(
    float3 TangentBasisX : TEXCOORD3;
    float3 TangentBasisY : TEXCOORD4;
    float3 TangentBasisZ : TEXCOORD5;
};


#include "Sampler.Normal.fx"
#include "Sampler.Primary.fx"
#include "Sampler.Secondary.fx"
#include "Sampler.Emissive.fx"
#include "Sampler.Occlusion.fx"

#include "PunctualLight.fx"
// #include "IBL.fx"
#include "PBR.Pixel.fx"

float4 PsShader(VsOutTexNorm input, bool hasPerturbedNormals, bool hasPrimary, bool hasSecondary, bool hasEmissive, bool hasOcclusionMap)
{
    // get primary color and alpha

    float4 f_primary = PrimaryScale * input.Color;
    if (hasPrimary) f_primary *= GetPrimarySample(input.TextureCoordinate0, input.TextureCoordinate1);

    // alpha cutoff
    clip((f_primary.a < AlphaCutoff) ? -1 : 1);

    // alpha blend
    f_primary.a = mad(f_primary.a, AlphaTransform.x, AlphaTransform.y);        

    // normals

    NormalInfo ninfo;

    if (hasPerturbedNormals) ninfo = GetPerturbedNormalSample(input);
    else ninfo = GetGeometricNormalSample(input);
    
    // calculate ambient light contribution

    float3 hdrColor = f_primary.rgb * AmbientLight;

    // calculate punctual lights contribution

    float4 f_secondary = 1;
    if (hasSecondary) f_secondary = GetSecondarySample(input.TextureCoordinate0, input.TextureCoordinate1);

    hdrColor += GetPunctualLightsContrib(input.PositionWS, ninfo, f_primary.rgb, f_secondary);

    // calculate emissive light contribution    

    float3 f_emissive = EmissiveScale;
    if (hasEmissive) f_emissive *= GetEmissiveSample(input.TextureCoordinate0, input.TextureCoordinate1);

    hdrColor += f_emissive;

    // calculate occlusion map attenuation
    
    if (hasOcclusionMap)
    {
        float f_occlusion = GetOcclusionSample(input.TextureCoordinate0, input.TextureCoordinate1);
        hdrColor = lerp(hdrColor, hdrColor * f_occlusion, OcclusionScale);
    }

    // all PBR lighting is calculated in linear RGB space (AKA HDR), we need to scale it down to sRGB    

    return float4(toneMap(hdrColor), f_primary.a);
}