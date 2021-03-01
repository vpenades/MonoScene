// based on: https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/tonemapping.glsl

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CONSTANTS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

float Exposure; // parameter for ToneMapping.toneMap

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// FUNCTIONS
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



static const float GAMMA = 2.2;
static const float INV_GAMMA = 1.0 / GAMMA;

// linear to sRGB approximation
// see http://chilliant.blogspot.com/2012/08/srgb-approximations-for-hlsl.html
float3 linearTosRGB(float3 lrgbIn)
{
    lrgbIn = max(lrgbIn, 0);

    return pow(lrgbIn, INV_GAMMA);
}

// sRGB to linear approximation
// see http://chilliant.blogspot.com/2012/08/srgb-approximations-for-hlsl.html
float3 sRGBToLinear(float3 srgbIn)
{
    srgbIn = max(srgbIn, 0);

    return pow(srgbIn, GAMMA);
}

float4 sRGBToLinear(float4 srgbIn)
{
    return float4(sRGBToLinear(srgbIn.rgb), srgbIn.a);
}

// Uncharted 2 tone map
// see: http://filmicworlds.com/blog/filmic-tonemapping-operators/
float3 toneMapUncharted2Impl(float3 color)
{
    const float A = 0.15;
    const float B = 0.50;
    const float C = 0.10;
    const float D = 0.20;
    const float E = 0.02;
    const float F = 0.30;
    return ((color * (A * color + C * B) + D * E) / (color * (A * color + B) + D * F)) - E / F;
}

float3 toneMapUncharted(float3 color)
{
    const float W = 11.2;
    color = toneMapUncharted2Impl(color * 2.0);
    float3 whiteScale = 1.0 / toneMapUncharted2Impl(W);
    return linearTosRGB(color * whiteScale);
}

// Hejl Richard tone map
// see: http://filmicworlds.com/blog/filmic-tonemapping-operators/
float3 toneMapHejlRichard(float3 color)
{
    color = max(float3(0,0,0), color - 0.004);
    return (color * (6.2 * color + .5)) / (color * (6.2 * color + 1.7) + 0.06);
}

// ACES tone map
// see: https://knarkowicz.wordpress.com/2016/01/06/aces-filmic-tone-mapping-curve/
float3 toneMapACES(float3 color)
{
    const float A = 2.51;
    const float B = 0.03;
    const float C = 2.43;
    const float D = 0.59;
    const float E = 0.14;
    return linearTosRGB(clamp((color * (A * color + B)) / (color * (C * color + D) + E), 0.0, 1.0));
}

float3 toneMap(float3 color)
{
    color *= Exposure;

#ifdef TONEMAP_UNCHARTED
    return toneMapUncharted(color);
#endif

#ifdef TONEMAP_HEJLRICHARD
    return toneMapHejlRichard(color);
#endif

#ifdef TONEMAP_ACES
    return toneMapACES(color);
#endif

    return linearTosRGB(color);
}