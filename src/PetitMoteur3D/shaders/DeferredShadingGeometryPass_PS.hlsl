#include "DeferredShadingGeometryPass_VSOut.hlsli"
#include "TextureHelper.hlsl"
#include "LightHelper.hlsl"

#ifndef DEFERRED_SHADING_GEOMETRY_PASS_PS
#define DEFERRED_SHADING_GEOMETRY_PASS_PS

struct LightParams
{
    float3 pos; // la position de la source d'éclairage (Point)
    float3 dir; // la direction de la source d'éclairage (Directionnelle)
    float4 vAEcl; // la valeur ambiante de l'éclairage
    float4 vDEcl; // la valeur diffuse de l'éclairage
    bool enable; // l'éclairage est allumé
    int3 offset; // Offset for memory alignment
};

cbuffer frameBuffer
{
    LightParams vLumiere; // la position de la source d'éclairage (Point)
    float4 vCamera; // la position de la caméra
}

cbuffer objectBuffer
{
    float4 vAMat; // la valeur ambiante du matériau
    float4 vDMat; // la valeur diffuse du matériau
    bool hasDiffuseTexture; // Has diffuse texture
    bool hasNormalTexture; // Has normal texture
    int2 offset; // Offset for memory alignment
}

SamplerState SampleState; // l'état de sampling
SamplerComparisonState ShadowMapSampler; // sampling pour la shadow map

Texture2D textureEntree; // la texture
Texture2D textureNormalMap; // la normal map

struct PS_OUT
{
    float4 LightAccumulation : SV_Target0; // Ambient + emissive (R8G8B8_????) Unused (A8_UNORM)
    float4 Diffuse : SV_Target1; // Diffuse Albedo (R8G8B8_UNORM) Unused (A8_UNORM)
    float4 Specular : SV_Target2; // Specular Color (R8G8B8_UNROM) Specular Power(A8_UNORM)
    float4 NormalVS : SV_Target3; // View space normal (R32G32B32_FLOAT) Unused (A32_FLOAT)
};

PS_OUT DeferredShadingGeometryPassPSImpl(DeferredShadingGeometryPass_VertexParams vertex, uniform bool useNormalMap, uniform bool useTextureColor)
{
    // Normal Geometry
    if (useNormalMap)
    {
        vertex.Norm = ReadNormalMap(textureNormalMap, SampleState, vertex.coordTex, vertex.Norm, vertex.Tang);
    }
    else
    {
        vertex.Norm = normalize(vertex.Norm);
    }

    // Normaliser les paramétres
    float3 N = normalize(vertex.Norm);
    float3 L = normalize(vertex.vDirLum);
    float3 V = normalize(vertex.vDirCam);
    // Valeur de la composante diffuse
    float3 diff = saturate(dot(N, L));
    // R = 2 * (N.L) * N é L
    float3 R = normalize(2 * diff.xyz * N - L);
    // Puissance de 4 - pour léexemple
    float S = pow(saturate(dot(R, V)), 64);

    // échantillonner la couleur du pixel é partir de la texture
    float3 couleurTexture;
    if (useTextureColor)
    {
        couleurTexture = ReadTextureRGB(textureEntree, SampleState, vertex.coordTex);
    }
    else
    {
        couleurTexture = (0.5f, 0.5f, 0.5f);
    }
    
    // I = A + D * N.L + (R.V)n
    float3 couleurLumiereAmbiante = 0.4f * vLumiere.vAEcl.rgb * vAMat.rgb;
    float3 couleurLumiereDiffuse = vLumiere.vDEcl.rgb * vDMat.rgb * diff;
    float3 couleurLumiereSpecular = 0.1f * S;
    
    PS_OUT result = (PS_OUT) 0;
    result.LightAccumulation = float4(couleurTexture * couleurLumiereAmbiante, 0);
    result.Diffuse = float4(couleurTexture * couleurLumiereDiffuse, 0);
    result.Specular = float4(couleurTexture * couleurLumiereSpecular, 0);
    result.NormalVS = float4(vertex.Norm, 0);
    return result;
}

// Render Techniques
PS_OUT DeferredShadingGeometryPassPS(DeferredShadingGeometryPass_VertexParams vertex)
{
    return DeferredShadingGeometryPassPSImpl(vertex, hasNormalTexture, hasDiffuseTexture);
}

#endif
