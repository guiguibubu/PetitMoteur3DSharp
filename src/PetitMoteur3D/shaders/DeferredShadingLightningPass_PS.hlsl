#include "DeferredShadingLightningPass_VSOut.hlsli"
#include "TextureHelper.hlsl"
#include "LightHelper.hlsl"

#ifndef DEFERRED_SHADING_LIGHTNING_PASS_PS
#define DEFERRED_SHADING_LIGHTNING_PASS_PS

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

SamplerState SampleState; // l'état de sampling

// The diffuse color from the view space texture.
Texture2D textureDiffuse : register(t0);
// The specular color from the screen space texture.
Texture2D textureSpecular : register(t1);
// The normal from the screen space texture.
Texture2D textureNormal : register(t2);

struct PS_OUT
{
    float4 color : SV_Target;
};

PS_OUT DeferredShadingLightningPassPSImpl(DeferredShadingLightningPass_VertexParams vertex)
{
    int2 texCoord = vertex.Pos.xy;
    float3 diffuse = textureDiffuse.Load(int3(texCoord, 0)).rgb;
    float4 specular = textureSpecular.Load(int3(texCoord, 0));
    float3 normal = textureNormal.Load(int3(texCoord, 0)).xyz;
    
    // Unpack the specular power from the alpha component of the specular color.
    float specularPower = exp2(specular.a * 10.5f);
    
    // View vector
    float3 V = normalize(vCamera - vertex.PosWorld).xyz;
    // Light shading
    DirectionalLigth light = (DirectionalLigth) 0;
    light.Direction = vLumiere.dir;
    light.Diffuse = vLumiere.vDEcl.rgb;
    light.Specular = vLumiere.vDEcl.rgb;
    LightingResult lit = DoDirectionalLight(light, specularPower, V, normal);
    
    float3 couleur = diffuse * lit.Diffuse + specular.rgb * lit.Specular;
    
    PS_OUT result = (PS_OUT) 0;
    result.color = float4(couleur, 1.0f);
    return result;
}

// Render Techniques
[earlydepthstencil]
PS_OUT DeferredShadingLightningPassPS(DeferredShadingLightningPass_VertexParams vertex)
{
    return DeferredShadingLightningPassPSImpl(vertex);
}

#endif
