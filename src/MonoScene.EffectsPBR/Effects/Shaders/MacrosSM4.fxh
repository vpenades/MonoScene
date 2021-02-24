
// Macros for targetting shader model 4.0 (DX11)

#ifdef OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
// https://github.com/MonoGame/MonoGame/issues/6219#issuecomment-369311408
 
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif

#define TECHNIQUE(name, vsname, psname ) \
	technique name { pass { VertexShader = compile VS_SHADERMODEL vsname(); PixelShader = compile PS_SHADERMODEL psname(); } }

#define BEGIN_CONSTANTS     cbuffer Parameters : register(b0) {
#define MATRIX_CONSTANTS
#define END_CONSTANTS       };

#define DECLARE_TEXTURE(Name, index) \
    Texture2D<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index)

#define DECLARE_CUBEMAP(Name, index) \
    TextureCube<float4> Name : register(t##index); \
    sampler Name##Sampler : register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)
#define SAMPLE_CUBEMAP(Name, texCoord)  Name.Sample(Name##Sampler, texCoord)