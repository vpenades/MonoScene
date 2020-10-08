
#include "Material.fx"
#include "PunctualContrib.fx"


MaterialInfo GetMaterialInfo(float3 normal, float3 primaryColor, float4 secondaryColor)
{
    MaterialInfo materialInfo;
    materialInfo.baseColor = primaryColor.rgb;

#ifdef MATERIAL_IOR
    float ior = u_IOR_and_f0.x;
    float f0_ior = u_IOR_and_f0.y;
#else
    // The default index of refraction of 1.5 yields a dielectric normal incidence reflectance of 0.04.
    float ior = 1.5;
    float f0_ior = 0.04;
#endif

#ifdef MATERIAL_SPECULARGLOSSINESS
    materialInfo = getSpecularGlossinessInfo(materialInfo, secondaryColor);
#elif MATERIAL_METALLICROUGHNESS
    materialInfo = getMetallicRoughnessInfo(materialInfo, f0_ior, secondaryColor);
#endif

    materialInfo.thickness = 1;
    materialInfo.absorption = 0;

    materialInfo.perceptualRoughness = clamp(materialInfo.perceptualRoughness, 0.0, 1.0);
    materialInfo.metallic = clamp(materialInfo.metallic, 0.0, 1.0);

    // Roughness is authored as perceptual roughness; as is convention,
    // convert to material roughness by squaring the perceptual roughness.
    materialInfo.alphaRoughness = materialInfo.perceptualRoughness * materialInfo.perceptualRoughness;

    // Compute reflectance.
    float reflectance = max(max(materialInfo.f0.r, materialInfo.f0.g), materialInfo.f0.b);

    // Anything less than 2% is physically impossible and is instead considered to be shadowing. Compare to "Real-Time-Rendering" 4th editon on page 325.
    materialInfo.f90 = clamp(reflectance * 50.0, 0.0, 1.0);

    materialInfo.n = normal;

    return materialInfo;
}


// https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/pbr.frag#L419
float3 GetPunctualLightsContrib(float3 positionW, NormalInfo normalInfo, MaterialInfo materialInfo)
{
    float3 v = normalize(CameraPosition - positionW);
    float3 n = normalInfo.n;
    float3 t = normalInfo.t;
    float3 b = normalInfo.b;

    float NdotV = clampedDot(n, v);
    float TdotV = clampedDot(t, v);
    float BdotV = clampedDot(b, v);    

    // lighting
    
    LightContrib result;
    result.f_diffuse = 0;
    result.f_specular = 0;

    for (int i = 0; i < 3; ++i)
    {
        PunctualLight lightSource = GetPunctualLight(i);
        LightContrib lightContrib = AggregateLight(lightSource, positionW, n, v, materialInfo);

        result.Add(lightContrib);
    }

    return result.f_diffuse + result.f_specular;
}