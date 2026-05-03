#include "DeferredShadingGeometryPass_VSOut.hlsli"
#include "TextureHelper.hlsl"
#include "LightHelper.hlsl"

#ifndef DEFERRED_SHADING_GEOMETRY_PASS_PS
#define DEFERRED_SHADING_GEOMETRY_PASS_PS

struct MaterialParams
{
    float4 ambiantColor; // la valeur ambiante du matériau
    float4 diffuseColor; // la valeur diffuse du matériau
    float4 specularColor; // la valeur specular du matériau
    float specularPower; // la valeur specular du matériau
    bool hasDiffuseTexture; // Has diffuse texture
    bool hasNormalTexture; // Has normal texture
    int offset; // Offset for memory alignment
};

struct LightParams
{
    float3 pos; // la position de la source d’éclairage (Point)
    float3 dir; // la direction de la source d’éclairage (Directionnelle)
    float4 vAEcl; // la valeur ambiante de l’éclairage
    float4 vDEcl; // la valeur diffuse de l’éclairage
    bool enable; // l'éclairage est allumé
    bool enableShadow; // autoriser ombre (ShadowMapping)
    int2 offset; // Offset for memory alignment
};

cbuffer frameBuffer
{
    LightParams vLumiere; // la position de la source d’éclairage (Point)
    float4 vCamera; // la position de la caméra
}

cbuffer objectBuffer
{
    MaterialParams vMaterial;
}

SamplerState SampleState; // l'état de sampling
SamplerComparisonState ShadowMapSampler; // sampling pour la shadow map

Texture2D textureDiffuse; // la texture
Texture2D textureNormalMap; // la normal map
Texture2D textureShadowMap; // la shadow map

struct PS_OUT
{
    float4 LightAccumulation : SV_Target0; // Ambient + emissive (R8G8B8_????) Unused (A8_UNORM)
    float4 Diffuse : SV_Target1; // Diffuse Albedo (R8G8B8_UNORM) Unused (A8_UNORM)
    float4 Specular : SV_Target2; // Specular Color (R8G8B8_UNROM) Specular Power(A8_UNORM)
    float4 NormalVS : SV_Target3; // View space normal (R32G32B32_FLOAT) Unused (A32_FLOAT)
    float4 Shadow : SV_Target4; // Shadow Mask (DXGI_FORMAT_R8_UNORM)
};

PS_OUT DeferredShadingGeometryPassPSImpl(DeferredShadingGeometryPass_VertexParams vertex, uniform bool useNormalMap, uniform bool useTextureColor, uniform bool drawShadow)
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
    
    float3 ambianteMaterial = vMaterial.ambiantColor.rgb;
    float3 diffuseMaterial = vMaterial.diffuseColor.rgb;
    float3 specularMaterial = vMaterial.specularColor.rgb;
    float specularPower = vMaterial.specularPower;
    // Method of packing specular power from "Deferred Rendering in Killzone 2" presentation 
    // from Michiel van der Leeuw, Guerrilla (2007)
    float specularPowerPacked = log2(specularPower) / 10.5f;
    
    // ********* OMBRE *********
    bool isInShadow = false;
    if (drawShadow)
    {
        // Conversion space [-1;1] to texture space [0;1]
        float2 shadowTexCoords = 0.5f * vertex.lightSpacePos.xy / vertex.lightSpacePos.w + float2(0.5f, 0.5f);
        shadowTexCoords.y = 1.0f - shadowTexCoords.y;
        float pixelDepth = vertex.lightSpacePos.z / vertex.lightSpacePos.w;

        // Check if the pixel texture coordinate is in the view frustum of the 
        // light before doing any shadow work.
        bool isInLightFrustum = saturate(shadowTexCoords.xy) == shadowTexCoords.xy;
        if (isInLightFrustum && pixelDepth > 0)
        {
            // Use an offset value to mitigate shadow artifacts due to imprecise 
            // floating-point values (shadow acne).
            //
            // This is an approximation of epsilon * tan(acos(saturate(NdotL))):
            float3 N = normalize(vertex.Norm);
            float3 L = normalize(vertex.vDirLum);
            float3 NdotL = saturate(dot(N, L));
            float margin = acos(NdotL);
#ifdef LINEAR
            // The offset can be slightly smaller with smoother shadow edges.
            float epsilon = 0.0005 / margin;
#else
            float epsilon = 0.001 / margin;
#endif
            // Clamp epsilon to a fixed range so it doesn't go overboard.
            epsilon = clamp(epsilon, 0, 0.1);

            // Use the SampleCmpLevelZero Texture2D method (or SampleCmp) to sample from 
            // the shadow map, just as you would with Direct3D feature level 10_0 and
            // higher.  Feature level 9_1 only supports LessOrEqual, which returns 0 if
            // the pixel is in the shadow.
            isInShadow = textureShadowMap.SampleCmpLevelZero(ShadowMapSampler, shadowTexCoords, pixelDepth + epsilon).x == 0;
        }
    }
    
    PS_OUT result = (PS_OUT) 0;
    result.LightAccumulation = float4(ambianteMaterial, 0);
    result.Diffuse = float4(couleurDiffuseTexture * diffuseMaterial, 0);
    result.Specular = float4(specularMaterial, specularPowerPacked);
    result.NormalVS = float4(vertex.Norm, 0);
    result.Shadow = isInShadow ? 255 : 0;
    return result;
}

// Render Techniques
[earlydepthstencil]
PS_OUT DeferredShadingGeometryPassPS(DeferredShadingGeometryPass_VertexParams vertex)
{
    return DeferredShadingGeometryPassPSImpl(vertex, vMaterial.hasNormalTexture, vMaterial.hasDiffuseTexture, vLumiere.enableShadow);
}

#endif
