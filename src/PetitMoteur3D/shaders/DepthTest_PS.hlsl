#include "DepthTest_VSOut.hlsli"

struct PS_OUT
{
    float4 color : SV_Target;
};

PS_OUT DepthTestPS(VS_Sortie vs)
{
    PS_OUT Out = (PS_OUT) 0;

    if (1.0f - vs.Profondeur.x <= 0.0f)
    {
        Out.color = float4(255, 0, 0, 1.0f);
    }
    else
    {
        Out.color = float4(0, 255, 0, 1.0f);
    }

    return Out;
}