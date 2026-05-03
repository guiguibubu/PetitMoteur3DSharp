#include "DeferredShadingLightningPass_VSOut.hlsli"

#ifndef DEFERRED_SHADING_LIGHTNING_PASS_VS
#define DEFERRED_SHADING_LIGHTNING_PASS_VS

cbuffer objectBuffer
{
    float4x4 matWorldViewProj; // la matrice totale
    float4x4 matWorld; // matrice de transformation dans le monde
}

DeferredShadingLightningPass_VertexParams DeferredShadingLightningPassVSImpl(uniform float4 Pos)
{
    DeferredShadingLightningPass_VertexParams sortie = (DeferredShadingLightningPass_VertexParams) 0;
    sortie.Pos = mul(Pos, matWorldViewProj);	
    sortie.PosWorld = mul(Pos, matWorld);
    return sortie;
}

// Render Techniques
DeferredShadingLightningPass_VertexParams DeferredShadingLightningPassVS(float4 Pos : POSITION)
{
    return DeferredShadingLightningPassVSImpl(Pos);
}

#endif
