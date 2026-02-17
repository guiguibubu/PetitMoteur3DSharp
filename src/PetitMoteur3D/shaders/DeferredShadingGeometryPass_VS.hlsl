#include "DeferredShadingGeometryPass_VSOut.hlsli"

#ifndef DEFERRED_SHADING_GEOMETRY_PASS_VS
#define DEFERRED_SHADING_GEOMETRY_PASS_VS

struct LightParams
{
    float3 pos; // la position de la source d’éclairage (Point)
    float3 dir; // la direction de la source d’éclairage (Directionnelle)
    float4 vAEcl; // la valeur ambiante de l’éclairage
    float4 vDEcl; // la valeur diffuse de l’éclairage
    bool enable; // l'éclairage est allumé
    int3 offset; // Offset for memory alignment
};

cbuffer frameBuffer
{
    LightParams vLumiere; // la position de la source d’éclairage (Point)
    float4 vCamera; // la position de la caméra
}

cbuffer objectBuffer
{
    float4x4 matWorldViewProj; // la matrice totale
    float4x4 matWorld; // matrice de transformation dans le monde
}

DeferredShadingGeometryPass_VertexParams DeferredShadingGeometryPassVSImpl(uniform float4 Pos, uniform float3 Normale, uniform float2 coordTex, uniform float3 Tangent)
{
    DeferredShadingGeometryPass_VertexParams sortie = (DeferredShadingGeometryPass_VertexParams) 0;
    sortie.Pos = mul(Pos, matWorldViewProj);
    sortie.Norm = mul(float4(Normale, 0.0f), matWorld).xyz;
    sortie.Tang = mul(float4(Tangent, 0.0f), matWorld).xyz;
    float3 PosWorld = mul(Pos, matWorld).xyz;
    if (vLumiere.dir.x != 0.0f || vLumiere.dir.y != 0.0f || vLumiere.dir.z != 0.0f)
    {
        sortie.vDirLum = -vLumiere.dir.xyz;
    }
    else
    {
        sortie.vDirLum = vLumiere.pos.xyz - PosWorld;
    }
    sortie.vDirCam = vCamera.xyz - PosWorld;

	// Coordonnées d’application de texture
    sortie.coordTex = coordTex;
	
    return sortie;
}

// Render Techniques
DeferredShadingGeometryPass_VertexParams DeferredShadingGeometryPassVS(float4 Pos : POSITION, float3 Normale : NORMAL, float2 coordTex : TEXCOORD, float3 Tangent : TANGENT)
{
    return DeferredShadingGeometryPassVSImpl(Pos, Normale, coordTex, Tangent);
}

#endif
