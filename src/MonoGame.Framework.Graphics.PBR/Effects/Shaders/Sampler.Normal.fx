
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

DECLARE_TEXTURE(NormalTexture, 0);

float NormalsMode; // ? > 1 : Forward; ? < -1 : Reverse;   ? == 0 : Auto

float NormalScale;
int NormalTextureIdx;
float3 NormalTransformU;
float3 NormalTransformV;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

NormalInfo GetGeometricNormalSample(VsOutTexNorm input)
{
    float3 v = normalize(CameraPosition - input.PositionWS);

    float3 ng = normalize(input.TangentBasisZ);

    // For a back-facing surface, the tangential basis vectors are negated.    
    
    float facing = step(NormalsMode, dot(v, ng)) * 2.0 - 1.0;
    ng *= facing;

    // result

    NormalInfo info;
    info.ng = ng;
    info.n = ng;
    info.t = 0; // should generate some random T & b ?
    info.b = 0;
    return info;
}



// https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/pbr.frag#L136
NormalInfo GetPerturbedNormalSample(VsOutTexNorm input)
{
    // create tangent basis:

    float3 v = normalize(CameraPosition - input.PositionWS);
    
    float3 t = normalize(input.TangentBasisX);
    float3 b = normalize(input.TangentBasisY);
    float3 ng = normalize(input.TangentBasisZ);    

    // float3 t = normalize(input.TangentBasis[0]);
    // float3 b = normalize(input.TangentBasis[1]);
    // float3 ng = normalize(input.TangentBasis[2]);

    // For a back-facing surface, the tangential basis vectors are negated.    
    
    float facing = step(NormalsMode, dot(v, ng)) * 2.0 - 1.0;
    ng *= facing;
    t *= facing;
    b *= facing;    

    // tangent basis
    float3x3 tangentBasis = float3x3(t, b, ng);

    // get normal sample

    float2 uv = NormalTextureIdx < 1 ? input.TextureCoordinate0 : input.TextureCoordinate1;
    float3 uvw = float3(uv, 1);
    uv.x = dot(uvw, NormalTransformU);
    uv.y = dot(uvw, NormalTransformV);

    float3 n = SAMPLE_TEXTURE(NormalTexture, uv).xyz * float3(2, 2, 2) - float3(1, 1, 1);

    // Compute pertubed normals:

    n *= float3(NormalScale.x, NormalScale.x, 1.0);
    n = mul(n, tangentBasis);
    n = normalize(n);

    // result

    NormalInfo info;
    info.ng = ng;
    info.t = t;
    info.b = b;
    info.n = n;
    return info;
}