

// https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/pbr.frag#L93

struct MaterialInfo
{
    float perceptualRoughness;      // roughness value, as authored by the model creator (input to shader)
    float3 f0;                        // full reflectance color (n incidence angle)

    float alphaRoughness;           // roughness mapped to a more linear change in the roughness (proposed by [2])
    float3 albedoColor;

    float3 f90;                       // reflectance color at grazing angle
    float metallic;

    float3 n; // normal
    float3 baseColor;

    float sheenIntensity;
    float3 sheenColor;
    float sheenRoughness;

    float anisotropy;

    float3 clearcoatF0;
    float3 clearcoatF90;
    float clearcoatFactor;
    float3 clearcoatNormal;
    float clearcoatRoughness;

    float subsurfaceScale;
    float subsurfaceDistortion;
    float subsurfacePower;
    float3 subsurfaceColor;
    float subsurfaceThickness;

    float thinFilmFactor;
    float thinFilmThickness;

    float thickness;

    float3 absorption;

    float transmission;
};




MaterialInfo getMetallicRoughnessInfo(MaterialInfo info, float f0_ior, float4 secondaryColor)
{
    // Roughness is stored in the 'g' channel, metallic is stored in the 'b' channel.
    // This layout intentionally reserves the 'r' channel for (optional) occlusion map data

    info.metallic = SecondaryScale.x * secondaryColor.b;
    info.perceptualRoughness = SecondaryScale.y * secondaryColor.g;

#ifdef MATERIAL_METALLICROUGHNESS_SPECULAROVERRIDE
    // Overriding the f0 creates unrealistic materials if the IOR does not match up.
    float3 f0 = getMetallicRoughnessSpecularFactor();
#else
    // Achromatic f0 based on IOR.
    float3 f0 = f0_ior;
#endif

    info.albedoColor = lerp(info.baseColor.rgb * (1.0 - f0), 0, info.metallic);
    info.f0 = lerp(f0, info.baseColor.rgb, info.metallic);

    return info;
}

MaterialInfo getSpecularGlossinessInfo(MaterialInfo info, float4 secondaryColor)
{
    info.f0 = SecondaryScale.xyz * secondaryColor.rgb;// specular
    info.perceptualRoughness = SecondaryScale.w * secondaryColor.a; // glossiness to roughness    

    info.perceptualRoughness = 1.0 - info.perceptualRoughness; // 1 - glossiness
    info.albedoColor = info.baseColor.rgb * (1.0 - max(max(info.f0.r, info.f0.g), info.f0.b));

    return info;
}