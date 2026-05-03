#include "MiniPhongNormalMapShadowMapDebugCam_VSOut.hlsl"

struct LightParams
{
    float3 pos; // la position de la source d’éclairage (Point)
    float3 dir; // la direction de la source d’éclairage (Directionnelle)
    float4 vAEcl; // la valeur ambiante de l’éclairage
    float4 vDEcl; // la valeur diffuse de l’éclairage
};

cbuffer frameBuffer : register(b0)
{
    LightParams vLumiere; // la position de la source d’éclairage (Point)
    float4 vCamera; // la position de la caméra
}

cbuffer objectBuffer : register(b1)
{
    float4x4 matWorldViewProj; // la matrice totale
    float4x4 matWorld; // matrice de transformation dans le monde
    float4 vAMat; // la valeur ambiante du matériau
    float4 vDMat; // la valeur diffuse du matériau
    bool hasTexture; // indique si a une texture
    bool hasNormalMap; // indique si a une normal map
}

cbuffer frameShadowBuffer : register(b2)
{
    bool drawShadow; // do we need to draw shadows
}

cbuffer frameDebugBuffer : register(b3)
{
    bool isDebugCamUsed; // scene drawn or debug cam
}

SamplerState SampleState : register(s0); // l’état de sampling
SamplerComparisonState ShadowMapSampler : register(s1); // sampling pour la shadow map
SamplerComparisonState DebugDepthMapSampler : register(s2); // sampling pour la debug depth map

Texture2D textureEntree : register(t0); // la texture
Texture2D textureNormalMap : register(t1); // la normal map
Texture2D textureShadowMap : register(t2); // la shadow map
Texture2D textureDebugDepthMap : register(t3); // la debug depth map

struct PS_OUT
{
    float4 color : SV_Target;
    float depth : SV_Depth;
};

PS_OUT MiniPhongNormalMapShadowMapDebugCamPS(VS_Sortie vs)
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
	
	// ********* OMBRE *********
    bool isInShadow = false;
    if (drawShadow)
    {
        // Conversion space [-1;1] to texture space [0;1]
        float2 shadowTexCoords = 0.5f * vs.lightSpacePos.xy / vs.lightSpacePos.w + float2(0.5f, 0.5f);
        shadowTexCoords.y = 1.0f - shadowTexCoords.y;
        float pixelDepth = vs.lightSpacePos.z / vs.lightSpacePos.w;

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
    
    float depth = 1.0f;
    if (isDebugCamUsed)
    {
        // Conversion space [-1;1] to texture space [0;1]
        float2 depthTexCoords = 0.5f * vs.gameSpacePos.xy / vs.gameSpacePos.w + float2(0.5f, 0.5f);
        depthTexCoords.y = 1.0f - depthTexCoords.y;
        float pixelDepth = vs.gameSpacePos.z / vs.gameSpacePos.w;

        // Check if the pixel texture coordinate is in the view frustum of the 
        // gameCam before doing any work.
        bool isGameCamFrustum = saturate(depthTexCoords.xy) == depthTexCoords.xy;
        if (isGameCamFrustum && pixelDepth > 0)
        {
            // Use the SampleCmpLevelZero Texture2D method (or SampleCmp) to sample from 
            // the shadow map, just as you would with Direct3D feature level 10_0 and
            // higher.  Feature level 9_1 only supports LessOrEqual, which returns 0 if
            // the pixel is in the shadow.
            float depthTexture = textureDebugDepthMap.Sample(SampleState, depthTexCoords).x;
            depth = min(depth, depthTexture);
        }
    }
    
    PS_OUT output = (PS_OUT) 0;
    output.color = float4(couleur, 1.0f);
    output.depth = depth;
    return output;
}
