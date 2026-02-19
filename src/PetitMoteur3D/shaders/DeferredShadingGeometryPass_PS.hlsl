#include "DeferredShadingGeometryPass_VSOut.hlsli"
#include "TextureHelper.hlsl"
#include "LightHelper.hlsl"

#ifndef DEFERRED_SHADING_GEOMETRY_PASS_PS
#define DEFERRED_SHADING_GEOMETRY_PASS_PS

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

Texture2D textureDiffuse; // la texture
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
        vertex.Norm = DoNormalMapping(textureNormalMap, SampleState, vertex.coordTex, vertex.Norm, vertex.Tang);
    }
    else
    {
        vertex.Norm = normalize(vertex.Norm);
    }

    // échantillonner la couleur du pixel é partir de la texture
    float3 couleurDiffuseTexture;
    if (useTextureColor)
    {
        couleurDiffuseTexture = ReadTextureRGB(textureDiffuse, SampleState, vertex.coordTex);
    }
    else
    {
        couleurDiffuseTexture = (0.5f, 0.5f, 0.5f);
    }
    
    float3 ambianteMaterial = vAMat.rgb;
    float3 diffuseMaterial = vDMat.rgb;
    float3 specularMaterial = (1,1,1);
    float specularPower = 64;
    // Method of packing specular power from "Deferred Rendering in Killzone 2" presentation 
    // from Michiel van der Leeuw, Guerrilla (2007)
    float specularPowerPacked = log2(specularPower) / 10.5f;
    
    
    PS_OUT result = (PS_OUT) 0;
    result.LightAccumulation = float4(ambianteMaterial, 0);
    result.Diffuse = float4(couleurDiffuseTexture * diffuseMaterial, 0);
    result.Specular = float4(specularMaterial, specularPowerPacked);
    result.NormalVS = float4(vertex.Norm, 0);
    return result;
}

// Render Techniques
[earlydepthstencil]
PS_OUT DeferredShadingGeometryPassPS(DeferredShadingGeometryPass_VertexParams vertex)
{
    return DeferredShadingGeometryPassPSImpl(vertex, hasNormalTexture, hasDiffuseTexture);
}

#endif
