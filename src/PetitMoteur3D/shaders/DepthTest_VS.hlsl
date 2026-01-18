#include "DepthTest_VSOut.hlsli"

cbuffer param : register(b0)
{
    float4x4 matWorldViewProj; // Matrice WVP
}

//-------------------------------------------------------------------
// Vertex Shader pour construire le shadow map
//-------------------------------------------------------------------
VS_Sortie DepthTestVS(float4 Pos : POSITION)
{
    VS_Sortie Out = (VS_Sortie) 0;

	// Calcul des coordonnées
    Out.Pos = mul(Pos, matWorldViewProj); // WVP

	// Obtenir la profondeur et normaliser avec w
    Out.Profondeur.x = Out.Pos.z / Out.Pos.w;

    return Out;
}