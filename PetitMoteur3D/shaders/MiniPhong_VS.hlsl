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

struct VS_Sortie
{
	float4 Pos : SV_Position;
	float3 Norm : TEXCOORD0;
	float3 vDirLum : TEXCOORD1;
	float3 vDirCam : TEXCOORD2;
	float2 coordTex : TEXCOORD3;
};

VS_Sortie MiniPhongVS(float4 Pos : POSITION, float3 Normale : NORMAL, float2 coordTex: TEXCOORD)
{
	VS_Sortie sortie = (VS_Sortie)0;
	sortie.Pos = mul(Pos, matWorldViewProj);
	sortie.Norm = mul(float4(Normale, 0.0f), matWorld).xyz;
	float3 PosWorld = mul(Pos, matWorld).xyz;
	sortie.vDirLum = vLumiere.pos.xyz - PosWorld;
	sortie.vDirCam = vCamera.xyz - PosWorld;

	// Coordonnées d’application de texture
	sortie.coordTex = coordTex;

	return sortie;
}
