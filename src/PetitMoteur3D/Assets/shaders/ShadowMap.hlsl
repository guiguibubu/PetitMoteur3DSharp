cbuffer param
{
    float4x4 matWorldViewProjLight; // Matrice WVP pour lumière
}

struct ShadowMapVS_SORTIE
{
    float4 Pos : SV_POSITION;
    float3 Profondeur : TEXCOORD0;
};

//-------------------------------------------------------------------
// Vertex Shader pour construire le shadow map
//-------------------------------------------------------------------
ShadowMapVS_SORTIE ShadowMapVS(float4 Pos : POSITION)
{
    ShadowMapVS_SORTIE Out = (ShadowMapVS_SORTIE) 0;

	// Calcul des coordonnées
    Out.Pos = mul(Pos, matWorldViewProjLight); // WVP de la lumiere

	// Obtenir la profondeur et normaliser avec w
    Out.Profondeur.x = Out.Pos.z / Out.Pos.w;

    return Out;
}