cbuffer frameBuffer
{
	float4 vLumiere; // la position de la source d’éclairage (Point)
	float4 vCamera; // la position de la caméra
	float4 vAEcl; // la valeur ambiante de l’éclairage
	float4 vDEcl; // la valeur diffuse de l’éclairage
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
	float3 Tang : TEXCOORD1;
	float3 vDirLum : TEXCOORD2;
	float3 vDirCam : TEXCOORD3;
	float2 coordTex : TEXCOORD4;
};

SamplerState SampleState; // l’état de sampling
Texture2D textureEntree; // la texture
Texture2D textureNormalMap; // la normal map

VS_Sortie MiniPhongNormalMapVS(float4 Pos : POSITION, float3 Normale : NORMAL, float2 coordTex: TEXCOORD, float3 Tangent : TANGENT)
{
	VS_Sortie sortie = (VS_Sortie)0;
	sortie.Pos = mul(Pos, matWorldViewProj);
	sortie.Norm = mul(float4(Normale, 0.0f), matWorld).xyz;
	sortie.Tang = mul(float4(Tangent, 0.0f), matWorld).xyz;
	float3 PosWorld = mul(Pos, matWorld).xyz;
	sortie.vDirLum = vLumiere.xyz - (PosWorld + (30.0f, 0.0f, 0.0f));
	sortie.vDirCam = vCamera.xyz - PosWorld;

	// Coordonnées d’application de texture
	sortie.coordTex = coordTex;

	return sortie;
}

float4 MiniPhongNormalMapPS( VS_Sortie vs ) : SV_Target0
{
	float3 couleur;

	if(hasNormalMap)
	{
		float3 normalMap = textureNormalMap.Sample(SampleState, vs.coordTex).rgb;

        //Change normal map range from [0, 1] to [-1, 1]
        normalMap = (2.0f*normalMap) - 1.0f;

        //Make sure tangent is completely orthogonal to normal
        vs.Tang = normalize(vs.Tang - dot(vs.Tang, vs.Norm)*vs.Norm);

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
	float S = pow(saturate(dot(R, V)), 16);

	// Échantillonner la couleur du pixel à partir de la texture
	float3 couleurTexture;
	if(hasTexture){
		couleurTexture = textureEntree.Sample(SampleState,
	vs.coordTex).rgb;
	}
	else{
		couleurTexture = ( 0.5f, 0.5f, 0.5f );
	}

	// I = A + D * N.L + (R.V)n
	float3 couleurLumiere = vAEcl.rgb * vAMat.rgb +
	vDEcl.rgb * vDMat.rgb * diff;
	couleur = couleurTexture * couleurLumiere;

	couleur += S;
	return float4(couleur, 1.0f);
}
