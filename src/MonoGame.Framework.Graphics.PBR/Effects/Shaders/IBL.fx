// based on: https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/ibl.glsl

// IBL

DECLARE_TEXTURE(u_GGXLUT, 5);

// uniform int u_MipCount;
uniform samplerCube u_LambertianEnvSampler;
uniform samplerCube u_GGXEnvSampler;

uniform samplerCube u_CharlieEnvSampler;
uniform sampler2D u_CharlieLUT;



float3 getIBLRadianceGGX(float3 n, float3 v, float perceptualRoughness, float3 specularColor)
{
    float NdotV = clampedDot(n, v);
    float lod = clamp(perceptualRoughness, 0.0, 1);
    float3 reflection = normalize(reflect(-v, n));

    float2 brdfSamplePoint = clamp(float2(NdotV, perceptualRoughness), float2(0.0, 0.0), float2(1.0, 1.0));
    float2 brdf = SAMPLE_TEXTURE(u_GGXLUT, brdfSamplePoint).rg;
    float4 specularSample = textureLod(u_GGXEnvSampler, reflection, lod);

    float3 specularLight = specularSample.rgb;

#ifndef USE_HDR
    specularLight = sRGBToLinear(specularLight);
#endif

    return specularLight * (specularColor * brdf.x + brdf.y);
}

float3 getIBLRadianceTransmission(float3 n, float3 v, float perceptualRoughness, float ior, float3 baseColor)
{
    // Sample GGX LUT.
    float NdotV = clampedDot(n, v);
    float2 brdfSamplePoint = clamp(float2(NdotV, perceptualRoughness), float2(0.0, 0.0), float2(1.0, 1.0));
    float2 brdf = texture(u_GGXLUT, brdfSamplePoint).rg;

    // Sample GGX environment map.
    float lod = clamp(perceptualRoughness * float(u_MipCount), 0.0, float(u_MipCount));

    // Approximate double refraction by assuming a solid sphere beneath the point.
    float3 r = refract(-v, n, 1.0 / ior);
    float3 m = 2.0 * dot(-n, r) * r + n;
    float3 rr = -refract(-r, m, ior);

    float4 specularSample = textureLod(u_GGXEnvSampler, rr, lod);
    float3 specularLight = specularSample.rgb;

#ifndef USE_HDR
    specularLight = sRGBToLinear(specularLight);
#endif

    return specularLight * (brdf.x + brdf.y);
}

float3 getIBLRadianceLambertian(float3 n, float3 diffuseColor)
{
    float3 diffuseLight = texture(u_LambertianEnvSampler, n).rgb;

#ifndef USE_HDR
    diffuseLight = sRGBToLinear(diffuseLight);
#endif

    return diffuseLight * diffuseColor;
}

float3 getIBLRadianceCharlie(float3 n, float3 v, float sheenRoughness, float3 sheenColor, float sheenIntensity)
{
    float NdotV = clampedDot(n, v);
    float lod = clamp(sheenRoughness * float(u_MipCount), 0.0, float(u_MipCount));
    float3 reflection = normalize(reflect(-v, n));

    float2 brdfSamplePoint = clamp(float2(NdotV, sheenRoughness), float2(0.0, 0.0), float2(1.0, 1.0));
    float brdf = texture(u_CharlieLUT, brdfSamplePoint).b;
    float4 sheenSample = textureLod(u_CharlieEnvSampler, reflection, lod);

    float3 sheenLight = sheenSample.rgb;

#ifndef USE_HDR
    sheenLight = sRGBToLinear(sheenLight);
#endif

    return sheenIntensity * sheenLight * sheenColor * brdf;
}

float3 getIBLRadianceSubsurface(float3 n, float3 v, float scale, float distortion, float power, float3 color, float thickness)
{
    float3 diffuseLight = texture(u_LambertianEnvSampler, n).rgb;

#ifndef USE_HDR
    diffuseLight = sRGBToLinear(diffuseLight);
#endif

    return diffuseLight * getPunctualRadianceSubsurface(n, v, -v, scale, distortion, power, color, thickness);
}