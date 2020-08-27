// https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/pbr.frag#L136
NormalInfo GetNormalSample(VsOutTexNorm input)
{
    // create tangent basis:

    float3 v = normalize(CameraPosition - input.PositionWS);
    float3 t = normalize(input.TangentBasisX);
    float3 b = normalize(input.TangentBasisY);
    float3 ng = normalize(input.TangentBasisZ);

    // For a back-facing surface, the tangential basis vectors are negated.
    float facing = step(0.0, dot(v, ng)) * 2.0 - 1.0;
    t *= facing;
    b *= facing;
    ng *= facing;

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

    NormalInfo info;
    info.ng = ng;
    info.t = t;
    info.b = b;
    info.n = n;
    return info;
}