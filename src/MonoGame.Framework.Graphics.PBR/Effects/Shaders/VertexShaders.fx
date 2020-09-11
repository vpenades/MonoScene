
// ========================================= CONSTANTS =========================================

#define SKINNED_EFFECT_MAX_BONES   128

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x3 Bones[SKINNED_EFFECT_MAX_BONES]; // 4x3 is enough, and saves constants 

// ========================================= STRUCTURES =========================================

struct VsInRigid
{
    float3 Position : POSITION0;
    float3 Normal : Normal0;

    float4 Color: COLOR0;
    float2 TextureCoordinate0 : TEXCOORD0;
    float2 TextureCoordinate1 : TEXCOORD1;
};

struct VsInSkinned
{
    float3 Position : POSITION0;
    float3 Normal : Normal0;

    float4 Color: COLOR0;
    float2 TextureCoordinate0 : TEXCOORD0;
    float2 TextureCoordinate1 : TEXCOORD1;

    uint4 BlendIndices : BLENDINDICES0;
    float4 BlendWeights : BLENDWEIGHT0;
};

struct VsInRigidTangent
{
    float3 Position : POSITION0;
    float3 Normal : Normal0;
    float4 Tangent: Tangent0;

    float4 Color: COLOR0;
    float2 TextureCoordinate0 : TEXCOORD0;
    float2 TextureCoordinate1 : TEXCOORD1;
};

struct VsInSkinnedTangent
{
    float3 Position : POSITION0;
    float3 Normal : Normal0;
    float4 Tangent: Tangent0;

    float4 Color: COLOR0;
    float2 TextureCoordinate0 : TEXCOORD0;
    float2 TextureCoordinate1 : TEXCOORD1;

    uint4 BlendIndices : BLENDINDICES0;
    float4 BlendWeights : BLENDWEIGHT0;
};


// ========================================= FUNCTIONS =========================================


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

    float3 normalW = mul(float4(input.Normal, 0.0), World).xyz;
    float3 tangentW = mul(float4(input.Tangent.xyz, 0.0), World).xyz;

    normalW = normalize(normalW);
    tangentW = normalize(tangentW);

    float3 bitangentW = cross(normalW, tangentW) * input.Tangent.w;

    return float3x3(tangentW, bitangentW, normalW);
}





VsOutTexNorm VsRigidBasis(VsInRigidTangent input)
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
    output.TextureCoordinate0 = input.TextureCoordinate0;
    output.TextureCoordinate1 = input.TextureCoordinate1;

    return output;
}

VsOutTexNorm VsSkinnedBasis(VsInSkinnedTangent input)
{

    float4x3 mbones = FunctionBoneMatrixCalculation(input.BlendIndices, input.BlendWeights);

    VsInRigidTangent output;

    output.Position = mul(float4(input.Position, 1.0f), mbones).xyz;
    output.Normal = mul(float4(input.Normal, 0), mbones).xyz;

    float3 tgt = mul(float4(input.Tangent.xyz, 0), mbones).xyz;
    output.Tangent = float4(tgt, input.Tangent.w);

    output.Color = input.Color;
    output.TextureCoordinate0 = input.TextureCoordinate0;
    output.TextureCoordinate1 = input.TextureCoordinate1;

    return VsRigidBasis(output);
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
    output.TangentBasisZ = mul(float4(input.Normal, 0.0), World).xyz;

    // output.TangentBasis[0] = float3(0, 0, 0);
    // output.TangentBasis[1] = float3(0, 0, 0);
    // output.TangentBasis[2] = mul(float4(input.Normal, 0.0), World).xyz;

    output.Color = input.Color;
    output.TextureCoordinate0 = input.TextureCoordinate0;
    output.TextureCoordinate1 = input.TextureCoordinate1;

    return output;
}

VsOutTexNorm VsSkinned(VsInSkinned input)
{
    float4x3 mbones = FunctionBoneMatrixCalculation(input.BlendIndices, input.BlendWeights);

    VsInRigid output;

    output.Position = mul(float4(input.Position, 1.0f), mbones).xyz;
    output.Normal = mul(float4(input.Normal, 0), mbones).xyz;

    output.Color = input.Color;
    output.TextureCoordinate0 = input.TextureCoordinate0;
    output.TextureCoordinate1 = input.TextureCoordinate1;

    return VsRigid(output);
}