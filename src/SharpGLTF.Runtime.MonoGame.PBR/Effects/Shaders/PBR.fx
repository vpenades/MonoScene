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
// STRUCTS
// will need different structs for all the possible vertex and pixel shader cases and the accompanying techniques.
// well switch to depending on the mesh parts available material.
// pbr doesn't make sense for things without metal roughness however we can default to a faked version.
// if that is desired.
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

struct VsInRigid
{
    float3 Position : POSITION0;
    float3 Normal : Normal0;    

    float4 Color: COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VsInSkinned
{
    float3 Position : POSITION0;
    float3 Normal : Normal0;    

    float4 Color: COLOR0;
    float2 TextureCoordinate : TEXCOORD0;

    uint4 BlendIndices : BLENDINDICES0;
    float4 BlendWeights : BLENDWEIGHT0;
};

struct VsInRigidTangent
{
    float3 Position : POSITION0;
    float3 Normal : Normal0;
    float4 Tangent: Tangent0;

    float4 Color: COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VsInSkinnedTangent
{
	float3 Position : POSITION0;
	float3 Normal : Normal0;
    float4 Tangent: Tangent0;

    float4 Color: COLOR0;
	float2 TextureCoordinate : TEXCOORD0;

	uint4 BlendIndices : BLENDINDICES0;
	float4 BlendWeights : BLENDWEIGHT0;
};

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



//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// VERTEX SHADERS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float4x3 FunctionBoneMatrixCalculation(float4 BlendIndices, float4 BlendWeights)
{
    float sum = BlendWeights.x + BlendWeights.y + BlendWeights.z + BlendWeights.w;

    float4x3 mbones =
        Bones[BlendIndices.x] * (float)BlendWeights.x / sum +
        Bones[BlendIndices.y] * (float)BlendWeights.y / sum +
        Bones[BlendIndices.z] * (float)BlendWeights.z / sum +
        Bones[BlendIndices.w] * (float)BlendWeights.w / sum;
    return mbones;
}

float3x3 GetTangentBasis(VsInRigidTangent input)
{
    // https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/35df3ff146e88cc585401db546fb9ee3366607d2/src/shaders/primitive.vert#L103

    float3 normalW = mul(float4(input.Normal, 0.0), World);
    float3 tangentW = mul(float4(input.Tangent.xyz, 0.0), World);

    normalW = normalize(normalW);
    tangentW = normalize(tangentW);    
    
    float3 bitangentW = cross(normalW, tangentW) * input.Tangent.w;

    return float3x3(tangentW, bitangentW, normalW);
}

VsOutTexNorm VsRigidNormal(VsInRigidTangent input)
{
    VsOutTexNorm output;    

    float4 pos = mul(float4(input.Position, 1.0f), World);    

    float4x4 vp = mul(View, Projection);
    
    output.PositionPS = mul(pos, vp);
    output.PositionWS = pos.xyz;
    
    float3x3 TBN = GetTangentBasis(input);
    // output.TangentBasis = TBN;    
    output.TangentBasisX = TBN[0];
    output.TangentBasisY = TBN[1];
    output.TangentBasisZ = TBN[2];

    output.Color = input.Color;
    output.TextureCoordinate = input.TextureCoordinate;   
    
    return output;
}

VsOutTexNorm VsSkinnedNormal(VsInSkinnedTangent input)
{
    
    float4x3 mbones = FunctionBoneMatrixCalculation(input.BlendIndices, input.BlendWeights);

    VsInRigidTangent output;
    output.Position = mul(float4(input.Position, 1.0f), mbones);

    output.Normal = mul(float4(input.Normal,0), mbones).xyz;

    float3 tgt = mul(float4(input.Tangent.xyz,0), mbones).xyz;
    output.Tangent = float4(tgt, input.Tangent.w);

    output.Color = input.Color;
    output.TextureCoordinate = input.TextureCoordinate;

    return VsRigidNormal(output);
}

VsOutTexNorm VsRigid(VsInRigid input)
{
    VsOutTexNorm output;

    float4 pos = mul(float4(input.Position, 1.0f), World);

    float4x4 vp = mul(View, Projection);

    output.PositionPS = mul(pos, vp);
    output.PositionWS = pos.xyz;

    output.TangentBasisX = float3(0, 0, 0);
    output.TangentBasisY = float3(0, 0, 0);
    output.TangentBasisZ = mul(float4(input.Normal, 0.0), World);

    output.Color = input.Color;
    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

VsOutTexNorm VsSkinned(VsInSkinned input)
{
    float4x3 mbones = FunctionBoneMatrixCalculation(input.BlendIndices, input.BlendWeights);

    VsInRigid output;
    output.Position = mul(float4(input.Position, 1.0f), mbones);

    output.Normal = mul(float4(input.Normal, 0), mbones).xyz;

    output.Color = input.Color;
    output.TextureCoordinate = input.TextureCoordinate;

    return VsRigid(output);
}




//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// PIXEL SHADERS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "PBR.Pixel.fx"

float4 PsWithGeometricNormal(VsOutTexNorm input) : COLOR0
{
    NormalInfo ninfo;
    ninfo.ng = normalize(input.TangentBasisZ);
    ninfo.n = ninfo.ng;
    ninfo.t = 0; // should generate some random T & b ?
    ninfo.b = 0;

    return PsWithPBR(input.PositionWS, input.Color, ninfo, input.TextureCoordinate);
}

float4 PsWithPerturbedNormal(VsOutTexNorm input) : COLOR0
{
    NormalInfo ninfo = getNormalInfo(input);

    return PsWithPBR(input.PositionWS, input.Color, ninfo, input.TextureCoordinate);
}



//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// TECHNIQUES
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

TECHNIQUE(Rigid, VsRigid, PsWithGeometricNormal );
TECHNIQUE(Skinned, VsSkinned, PsWithGeometricNormal );

TECHNIQUE(RigidNormal, VsRigidNormal, PsWithPerturbedNormal );
TECHNIQUE(SkinnedNormal, VsSkinnedNormal, PsWithPerturbedNormal );