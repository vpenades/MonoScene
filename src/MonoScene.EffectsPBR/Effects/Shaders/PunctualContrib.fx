//-------------- Light aggregation

struct LightContrib
{
    float3 f_diffuse;
    float3 f_specular;    

#ifdef MATERIAL_CLEARCOAT
    float3 f_clearcoat;
#endif

#ifdef MATERIAL_SHEEN
    float3 f_sheen;
#endif

#ifdef MATERIAL_SUBSURFACE
    float3 f_subsurface;
#endif

#ifdef MATERIAL_TRANSMISSION
    float3 f_transmission;
#endif

    void Add(LightContrib b)
    {
        this.f_diffuse += b.f_diffuse;
        this.f_specular += b.f_specular;        

#ifdef MATERIAL_CLEARCOAT
        this.f_clearcoat += b.f_clearcoat;
#endif

#ifdef MATERIAL_SHEEN
        this.f_sheen += b.f_sheen;
#endif

#ifdef MATERIAL_SUBSURFACE
        this.f_subsurface += b.f_subsurface;
#endif

#ifdef MATERIAL_TRANSMISSION
        this.f_transmission += b.f_transmission;
#endif
    }
};



// https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/pbr.frag#L545
LightContrib AggregateLight(PunctualLight light, float3 positionW, float3 n, float3 v, MaterialInfo materialInfo)
{
    float3 pointToLight = light.GetPointToLightVector(positionW);
    float3 intensity = light.GetIntensity(pointToLight);

    float3 l = normalize(pointToLight);   // Direction from surface point to light
    float3 h = normalize(l + v);          // Direction of the vector between l and v, called halfway vector
    float NdotL = clampedDot(n, l);
    float NdotV = clampedDot(n, v);
    float NdotH = clampedDot(n, h);
    float LdotH = clampedDot(l, h);
    float VdotH = clampedDot(v, h);


    LightContrib result;
    result.f_specular = 0;
    result.f_diffuse = 0;

    if (NdotL > 0.0 || NdotV > 0.0)
    {
        // Calculation of analytical light
        //https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#acknowledgments AppendixB
        result.f_diffuse = intensity * NdotL * BRDF_lambertian(materialInfo.f0, materialInfo.f90, materialInfo.albedoColor, VdotH);

#ifdef MATERIAL_ANISOTROPY
        float3 h = normalize(l + v);
        float TdotL = dot(t, l);
        float BdotL = dot(b, l);
        float TdotH = dot(t, h);
        float BdotH = dot(b, h);
        result.f_specular = intensity * NdotL * BRDF_specularAnisotropicGGX(materialInfo.f0, materialInfo.f90, materialInfo.alphaRoughness,
            VdotH, NdotL, NdotV, NdotH,
            BdotV, TdotV, TdotL, BdotL, TdotH, BdotH, materialInfo.anisotropy);
#else
        result.f_specular = intensity * NdotL * BRDF_specularGGX(materialInfo.f0, materialInfo.f90, materialInfo.alphaRoughness, VdotH, NdotL, NdotV, NdotH);
#endif

#ifdef MATERIAL_SHEEN
        result.f_sheen = intensity * getPunctualRadianceSheen(materialInfo.sheenColor, materialInfo.sheenIntensity, materialInfo.sheenRoughness,
            NdotL, NdotV, NdotH);
#endif

#ifdef MATERIAL_CLEARCOAT
        result.f_clearcoat = intensity * getPunctualRadianceClearCoat(materialInfo.clearcoatNormal, v, l,
            h, VdotH,
            materialInfo.clearcoatF0, materialInfo.clearcoatF90, materialInfo.clearcoatRoughness);
#endif
    }

#ifdef MATERIAL_SUBSURFACE
    result.f_subsurface = intensity * getPunctualRadianceSubsurface(n, v, l,
        materialInfo.subsurfaceScale, materialInfo.subsurfaceDistortion, materialInfo.subsurfacePower,
        materialInfo.subsurfaceColor, materialInfo.subsurfaceThickness);
#endif

#ifdef MATERIAL_TRANSMISSION
    result.f_transmission = intensity * getPunctualRadianceTransmission(n, v, l, materialInfo.alphaRoughness, ior, materialInfo.f0);
#endif

    return result;
}