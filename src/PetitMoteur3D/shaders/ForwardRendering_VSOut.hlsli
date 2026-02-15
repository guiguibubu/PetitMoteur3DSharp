struct ForwardRendering_VertexParams
{
    float4 Pos : SV_Position;
    float3 Norm : TEXCOORD0;
    float3 Tang : TEXCOORD1;
    float3 vDirLum : TEXCOORD2;
    float3 vDirCam : TEXCOORD3;
    float2 coordTex : TEXCOORD4;
    float4 lightSpacePos : TEXCOORD5;
};
