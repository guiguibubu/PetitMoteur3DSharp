struct ForwardRendering_VertexParams
{
    float4 Pos : SV_Position;
    float3 Norm : NORMAL;
    float3 Tang : TANGENT;
    float3 vDirLum : TEXCOORD1;
    float3 vDirCam : TEXCOORD2;
    float2 coordTex : TEXCOORD3;
    float4 lightSpacePos : TEXCOORD4;
};
