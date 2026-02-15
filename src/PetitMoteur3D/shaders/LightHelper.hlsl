#include "TextureHelper.hlsl"

#ifndef LIGHT_HELPER
#define LIGHT_HELPER

float3 ReadNormalMap(Texture2D textureNormalMap, SamplerState samplerTexture, float2 coordTex, float3 normalVertex, float3 tangentVertex)
{
    float3 normalMap = ReadTextureRGB(textureNormalMap, samplerTexture, coordTex);

    //Change normal map range from [0, 1] to [-1, 1]
    normalMap = (2.0f * normalMap) - 1.0f;

    //Make sure tangent is completely orthogonal to normal
    tangentVertex = normalize(tangentVertex - dot(tangentVertex, normalVertex) * normalVertex);

    //Create the biTangent
    float3 biTangent = cross(normalVertex, tangentVertex);

    //Create the "Texture Space"
    float3x3 texSpace = float3x3(tangentVertex, biTangent, normalVertex);

    //Convert normal from normal map to texture space and store in input.normal
    return normalize(mul(normalMap, texSpace));
}

#endif
