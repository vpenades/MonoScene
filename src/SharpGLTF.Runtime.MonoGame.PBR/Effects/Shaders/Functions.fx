// based on https://github.com/KhronosGroup/glTF-Sample-Viewer/blob/master/src/shaders/functions.glsl
// textures.fx needs to be included

static const float M_PI = 3.141592653589793;

/*
in vec3 v_Position;

#ifdef HAS_NORMALS
#ifdef HAS_TANGENTS
in mat3 v_TBN;
#else
in vec3 v_Normal;
#endif
#endif

#ifdef HAS_VERTEX_COLOR_VEC3
in vec3 v_Color;
#endif
#ifdef HAS_VERTEX_COLOR_VEC4
in vec4 v_Color;
#endif

vec4 getVertexColor()
{
    vec4 color = vec4(1.0, 1.0, 1.0, 1.0);

#ifdef HAS_VERTEX_COLOR_VEC3
    color.rgb = v_Color;
#endif
#ifdef HAS_VERTEX_COLOR_VEC4
    color = v_Color;
#endif

    return color;
}*/

struct NormalInfo {
    float3 ng;   // Geometric normal
    float3 n;    // Pertubed normal
    float3 t;    // Pertubed tangent
    float3 b;    // Pertubed bitangent
};

float clampedDot(float3 x, float3 y)
{
    return clamp(dot(x, y), 0.0, 1.0);
}

float sq(float t)
{
    return t * t;
}

float2 sq(float2 t)
{
    return t * t;
}

float3 sq(float3 t)
{
    return t * t;
}

float4 sq(float4 t)
{
    return t * t;
}

float3 transmissionAbsorption(float3 v, float3 n, float ior, float thickness, float3 absorptionColor)
{
    float3 r = refract(-v, n, 1.0 / ior);
    return exp(-absorptionColor * thickness * dot(-n, r));
}