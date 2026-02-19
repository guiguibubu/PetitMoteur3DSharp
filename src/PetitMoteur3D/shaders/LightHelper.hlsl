#include "TextureHelper.hlsl"

#ifndef LIGHT_HELPER
#define LIGHT_HELPER

float3x3 CalculateTBN(float3 normal, float3 tangent)
{
    //Make sure tangent is completely orthogonal to normal
    tangent = normalize(tangent - dot(tangent, normal) * normal);

    //Create the biTangent
    float3 biTangent = cross(normal, tangent);

    //Create the "Texture Space"
    return float3x3(tangent, biTangent, normal);
}

float3 DoNormalMapping(Texture2D textureNormalMap, SamplerState samplerTexture, float2 coordTex, float3x3 TBN)
{
    float3 normalMap = ReadTextureRGB(textureNormalMap, samplerTexture, coordTex);

    //Change normal map range from [0, 1] to [-1, 1]
    normalMap = (2.0f * normalMap) - 1.0f;

    //Convert normal from normal map to texture space
    return normalize(mul(normalMap, TBN));
}

float3 DoNormalMapping(Texture2D textureNormalMap, SamplerState samplerTexture, float2 coordTex, float3 normalVertex, float3 tangentVertex)
{
    //Create the "Texture Space"
    float3x3 texSpace = CalculateTBN(normalVertex, tangentVertex);

    //Convert normal from normal map to texture space
    return DoNormalMapping(textureNormalMap, samplerTexture, coordTex, texSpace);
}

float3 DoDiffuse(float3 lightDiffuseColor, float3 L, float3 N)
{
    float NdotL = max(dot(N, L), 0);
    return lightDiffuseColor * NdotL;
}

float3 DoSpecular(float3 lightSpecularColor, float specularPower, float3 V, float3 L, float3 N)
{
    float3 R = normalize(reflect(-L, N));
    float RdotV = max(dot(R, V), 0);

    return lightSpecularColor * pow(RdotV, specularPower);
}

struct LightingResult
{
    float3 Diffuse;
    float3 Specular;
};

struct DirectionalLigth
{
    float3 Direction;
    float3 Diffuse;
    float3 Specular;
};

LightingResult DoDirectionalLight(DirectionalLigth light, float specularPower, float3 V, float3 N)
{
    LightingResult result;

    float3 L = normalize(-light.Direction);

    result.Diffuse = DoDiffuse(light.Diffuse, L, N);
    result.Specular = DoSpecular(light.Specular, specularPower, V, L, N);

    return result;
}

#endif
