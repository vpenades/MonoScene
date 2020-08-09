// Based on: https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/punctual.glsl

static const int LightType_Directional = 0;
static const int LightType_Point = 1;
static const int LightType_Spot = 2;

struct Light
{
    float3 direction;
    float range;

    float3 color;
    float intensity;

    float3 position;
    float innerConeCos;

    float outerConeCos;
    int type;

    float2 padding;    
};



// https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_lights_punctual/README.md#range-property
float getRangeAttenuation(float range, float distance)
{
    if (range <= 0.0)
    {
        // negative range means unlimited
        return 1.0;
    }
    return max(min(1.0 - pow(distance / range, 4.0), 1.0), 0.0) / pow(distance, 2.0);
}

// https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_lights_punctual/README.md#inner-and-outer-cone-angles
float getSpotAttenuation(float3 pointToLight, float3 spotDirection, float outerConeCos, float innerConeCos)
{
    float actualCos = dot(normalize(spotDirection), normalize(-pointToLight));
    if (actualCos > outerConeCos)
    {
        if (actualCos < innerConeCos)
        {
            return smoothstep(outerConeCos, innerConeCos, actualCos);
        }
        return 1.0;
    }
    return 0.0;
}

float3 getPunctualRadianceSubsurface(float3 n, float3 v, float3 l, float scale, float distortion, float power, float3 color, float thickness)
{
    float3 distortedHalfway = l + n * distortion;
    float backIntensity = max(0.0, dot(v, -distortedHalfway));
    float reverseDiffuse = pow(clamp(0.0, 1.0, backIntensity), power) * scale;
    return (reverseDiffuse + color) * (1.0 - thickness);
}

float3 getPunctualRadianceTransmission(float3 n, float3 v, float3 l, float alphaRoughness, float ior, float3 f0)
{
    float3 r = refract(-v, n, 1.0 / ior);
    float3 h = normalize(l - r);
    float NdotL = clampedDot(-n, l);
    float NdotV = clampedDot(n, -r);

    float Vis = V_GGX(clampedDot(-n, l), NdotV, alphaRoughness);
    float D = D_GGX(clampedDot(r, l), alphaRoughness);

    return NdotL * f0 * Vis * D;
}

float3 getPunctualRadianceClearCoat(float3 clearcoatNormal, float3 v, float3 l, float3 h, float VdotH, float3 f0, float3 f90, float clearcoatRoughness)
{
    float NdotL = clampedDot(clearcoatNormal, l);
    float NdotV = clampedDot(clearcoatNormal, v);
    float NdotH = clampedDot(clearcoatNormal, h);
    return NdotL * BRDF_specularGGX(f0, f90, clearcoatRoughness * clearcoatRoughness, VdotH, NdotL, NdotV, NdotH);
}

float3 getPunctualRadianceSheen(float3 sheenColor, float sheenIntensity, float sheenRoughness, float NdotL, float NdotV, float NdotH)
{
    return NdotL * BRDF_specularSheen(sheenColor, sheenIntensity, sheenRoughness, NdotL, NdotV, NdotH);
}

