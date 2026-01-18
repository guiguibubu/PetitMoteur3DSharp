#include "MiniPhongNormalMap_VS_Sortie.hlsl"

struct LightParams
{
    float3 pos; // la position de la source d’éclairage (Point)
    float3 dir; // la direction de la source d’éclairage (Directionnelle)
    float4 vAEcl; // la valeur ambiante de l’éclairage
    float4 vDEcl; // la valeur diffuse de l’éclairage
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
    float4 vAMat; // la valeur ambiante du matériau
    float4 vDMat; // la valeur diffuse du matériau
    bool hasTexture; // indique si a une texture
    bool hasNormalMap; // indique si a une normal map
}

SamplerState SampleState; // l’état de sampling
Texture2D textureEntree; // la texture
Texture2D textureNormalMap; // la normal map

float4 MiniPhongNormalMapPS(VS_Sortie vs) : SV_Target0
{
    float3 couleur;

    if (hasNormalMap)
    {
        float3 normalMap = textureNormalMap.Sample(SampleState, vs.coordTex).rgb;

        //Change normal map range from [0, 1] to [-1, 1]
        normalMap = (2.0f * normalMap) - 1.0f;

        //Make sure tangent is completely orthogonal to normal
        vs.Tang = normalize(vs.Tang - dot(vs.Tang, vs.Norm) * vs.Norm);

        //Create the biTangent
        float3 biTangent = cross(vs.Norm, vs.Tang);

        //Create the "Texture Space"
        float3x3 texSpace = float3x3(vs.Tang, biTangent, vs.Norm);

        //Convert normal from normal map to texture space and store in input.normal
        vs.Norm = normalize(mul(normalMap, texSpace));
    }

	// Normaliser les paramètres
    float3 N = normalize(vs.Norm);
    float3 L = normalize(vs.vDirLum);
    float3 V = normalize(vs.vDirCam);
	// Valeur de la composante diffuse
    float3 diff = saturate(dot(N, L));
	// R = 2 * (N.L) * N – L
    float3 R = normalize(2 * diff.xyz * N - L);
	// Puissance de 4 - pour l’exemple
    float S = pow(saturate(dot(R, V)), 64);

	// Échantillonner la couleur du pixel à partir de la texture
    float3 couleurTexture;
    if (hasTexture)
    {
        couleurTexture = textureEntree.Sample(SampleState,
	vs.coordTex).rgb;
    }
    else
    {
        couleurTexture = (0.5f, 0.5f, 0.5f);
    }

	// I = A + D * N.L + (R.V)n
    float3 couleurLumiere = 0.1f * vLumiere.vAEcl.rgb * vAMat.rgb +
	vLumiere.vDEcl.rgb * vDMat.rgb * diff;
    couleur = couleurTexture * couleurLumiere;

    couleur += 0.1f * S;
    return float4(couleur, 1.0f);
}
