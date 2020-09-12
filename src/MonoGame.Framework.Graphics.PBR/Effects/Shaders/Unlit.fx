//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Shader globals
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

// https://github.com/KhronosGroup/glTF-Sample-Viewer/tree/master/src/shaders

#include "MacrosSM4.fxh"
#include "ToneMapping.fx"
#include "Alpha.fx"

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float3 CameraPosition;

#include "BasicFog.fx"

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



#include "Sampler.Primary.fx"
#include "Sampler.Emissive.fx"
#include "Sampler.Occlusion.fx"

float4 PsShader(VsOutTexNorm input, bool hasPrimary, bool hasEmissive, bool hasOcclusion)
{
    // get primary color and alpha

    float4 f_primary = PrimaryScale * input.Color;
    if (hasPrimary) f_primary *= GetPrimarySample(input.TextureCoordinate0, input.TextureCoordinate1);

    f_primary.a = ProcessAlphaChannel(f_primary.a); // alpha Cutoff & Blend
    f_primary.rgb = sRGBToLinear(f_primary.rgb); // gamma correction

    // primary color contribution

    float3 linearColor = f_primary.rgb;

    // calculate emissive light contribution    

    float3 f_emissive = EmissiveScale;
    if (hasEmissive) f_emissive *= GetEmissiveSample(input.TextureCoordinate0, input.TextureCoordinate1);

    linearColor += sRGBToLinear(f_emissive);
    
    // calculate occlusion map attenuation

    if (hasOcclusion)
    {
        float f_occlusion = GetOcclusionSample(input.TextureCoordinate0, input.TextureCoordinate1);
        linearColor = lerp(linearColor, linearColor * f_occlusion, OcclusionScale);
    }

    // all PBR lighting is calculated in linear RGB space (AKA HDR), we need to scale it down to sRGB    

    float4 sRGBA = float4(toneMap(linearColor), f_primary.a);

    // apply athmospheric effecs

    ApplyFog(sRGBA, input.PositionWS);

    return sRGBA;
}