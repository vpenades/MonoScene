

// https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/pbr.frag#L419
float4 PsWithPBR(float3 positionW, NormalInfo normalInfo, float2 uv)
{
    float4 baseColor = getBaseColor(uv, 1);

    float3 v = normalize(CameraPosition - positionW);
    float3 n = normalInfo.n;
    float3 t = normalInfo.t;
    float3 b = normalInfo.b;

    float NdotV = clampedDot(n, v);
    float TdotV = clampedDot(t, v);
    float BdotV = clampedDot(b, v);

    MaterialInfo materialInfo;
    materialInfo.baseColor = baseColor.rgb;

#ifdef MATERIAL_IOR
    float ior = u_IOR_and_f0.x;
    float f0_ior = u_IOR_and_f0.y;
#else
    // The default index of refraction of 1.5 yields a dielectric normal incidence reflectance of 0.04.
    float ior = 1.5;
    float f0_ior = 0.04;
#endif

    materialInfo = getMetallicRoughnessInfo(materialInfo, f0_ior, uv);

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

    materialInfo.n = n;

    // lighting

    // LIGHTING
    float3 f_specular = 0;
    float3 f_diffuse = 0;
    float3 f_emissive = 0;
    float3 f_clearcoat = 0;
    float3 f_sheen = 0;
    float3 f_subsurface = 0;
    float3 f_transmission = 0;

    for (int i = 0; i < 3; ++i)
    {
        Light light = getLight(i);

        float3 pointToLight = -light.direction;
        float rangeAttenuation = 1.0;
        float spotAttenuation = 1.0;

        if (light.type != LightType_Directional)
        {
            pointToLight = light.position - positionW;
        }

        // Compute range and spot light attenuation.
        if (light.type != LightType_Directional)
        {
            rangeAttenuation = getRangeAttenuation(light.range, length(pointToLight));
        }
        if (light.type == LightType_Spot)
        {
            spotAttenuation = getSpotAttenuation(pointToLight, light.direction, light.outerConeCos, light.innerConeCos);
        }

        float3 intensity = rangeAttenuation * spotAttenuation * light.intensity * light.color;

        float3 l = normalize(pointToLight);   // Direction from surface point to light
        float3 h = normalize(l + v);          // Direction of the vector between l and v, called halfway vector
        float NdotL = clampedDot(n, l);
        float NdotV = clampedDot(n, v);
        float NdotH = clampedDot(n, h);
        float LdotH = clampedDot(l, h);
        float VdotH = clampedDot(v, h);

        if (NdotL > 0.0 || NdotV > 0.0)
        {
            // Calculation of analytical light
            //https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#acknowledgments AppendixB
            f_diffuse += intensity * NdotL * BRDF_lambertian(materialInfo.f0, materialInfo.f90, materialInfo.albedoColor, VdotH);

#ifdef MATERIAL_ANISOTROPY
            float3 h = normalize(l + v);
            float TdotL = dot(t, l);
            float BdotL = dot(b, l);
            float TdotH = dot(t, h);
            float BdotH = dot(b, h);
            f_specular += intensity * NdotL * BRDF_specularAnisotropicGGX(materialInfo.f0, materialInfo.f90, materialInfo.alphaRoughness,
                VdotH, NdotL, NdotV, NdotH,
                BdotV, TdotV, TdotL, BdotL, TdotH, BdotH, materialInfo.anisotropy);
#else
            f_specular += intensity * NdotL * BRDF_specularGGX(materialInfo.f0, materialInfo.f90, materialInfo.alphaRoughness, VdotH, NdotL, NdotV, NdotH);
#endif

#ifdef MATERIAL_SHEEN
            f_sheen += intensity * getPunctualRadianceSheen(materialInfo.sheenColor, materialInfo.sheenIntensity, materialInfo.sheenRoughness,
                NdotL, NdotV, NdotH);
#endif

#ifdef MATERIAL_CLEARCOAT
            f_clearcoat += intensity * getPunctualRadianceClearCoat(materialInfo.clearcoatNormal, v, l,
                h, VdotH,
                materialInfo.clearcoatF0, materialInfo.clearcoatF90, materialInfo.clearcoatRoughness);
#endif
        }

#ifdef MATERIAL_SUBSURFACE
        f_subsurface += intensity * getPunctualRadianceSubsurface(n, v, l,
            materialInfo.subsurfaceScale, materialInfo.subsurfaceDistortion, materialInfo.subsurfacePower,
            materialInfo.subsurfaceColor, materialInfo.subsurfaceThickness);
#endif

#ifdef MATERIAL_TRANSMISSION
        f_transmission += intensity * getPunctualRadianceTransmission(n, v, l, materialInfo.alphaRoughness, ior, materialInfo.f0);
#endif
    }

    // blending

    float3 color = (f_emissive + f_diffuse + f_specular);

    float ao = SAMPLE_TEXTURE(OcclusionTexture, uv).x;
    color = lerp(color, color * ao, OcclusionScale.x);

    return float4(toneMap(color), 1);
}