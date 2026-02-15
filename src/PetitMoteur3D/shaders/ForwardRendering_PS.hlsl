#include "ForwardRendering_VSOut.hlsli"
#include "TextureHelper.hlsl"
#include "LightHelper.hlsl"

#ifndef FORWARD_RENDERING_PS
#define FORWARD_RENDERING_PS

struct LightParams
{
    float3 pos; // la position de la source d'éclairage (Point)
    float3 dir; // la direction de la source d'éclairage (Directionnelle)
    float4 vAEcl; // la valeur ambiante de l'éclairage
    float4 vDEcl; // la valeur diffuse de l'éclairage
    bool enable; // l'éclairage est allumé
    bool enableShadow; // autoriser ombre (ShadowMapping)
    int2 offset; // Offset for memory alignment
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
Texture2D textureShadowMap; // la shadow map

struct PS_OUT
{
    float4 color : SV_Target;
};

PS_OUT ForwardRenderingPSImpl(ForwardRendering_VertexParams vertex, uniform bool useNormalMap, uniform bool useTextureColor, uniform bool drawShadow)
{
    float3 couleur;

    if (useNormalMap)
    {
        vertex.Norm = ReadNormalMap(textureNormalMap, SampleState, vertex.coordTex, vertex.Norm, vertex.Tang);
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
            float margin = acos(diff);
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

    // I = A + D * N.L + (R.V)n
    float3 couleurLumiereAmbiante = 0.4f * vLumiere.vAEcl.rgb * vAMat.rgb;
    float3 couleurLumiereDiffuse = vLumiere.vDEcl.rgb * vDMat.rgb * diff;
    float3 couleurLumiereSpecular = 0.1f * S;
    // If in shadow
    if (isInShadow)
    {
        couleur = couleurTexture * (couleurLumiereAmbiante);
    }
    else
    {
        couleur = couleurTexture * (couleurLumiereAmbiante + couleurLumiereDiffuse + couleurLumiereSpecular);
    }
    
    PS_OUT result = (PS_OUT) 0;
    result.color = float4(couleur, 1.0f);
    return result;
}

// Render Techniques
PS_OUT ForwardRenderingPS(ForwardRendering_VertexParams vertex)
{
    return ForwardRenderingPSImpl(vertex, hasNormalTexture, hasDiffuseTexture, vLumiere.enableShadow);
}

#endif
