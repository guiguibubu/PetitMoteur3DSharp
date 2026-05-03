#include "DepthTest_VSOut.hlsli"

cbuffer param : register(b0)
{
    float4 successColor; // la valeur ambiante du matériau en cas de succes
    float4 failColor; // la valeur ambiante du matériau en cas de d'échec
}

struct PS_OUT
{
    float4 color : SV_Target;
    float depth : SV_Depth;
};

PS_OUT DepthTestPS(VS_Sortie vs)
{
    PS_OUT Out = (PS_OUT) 0;

    if (1.0f - vs.Profondeur.x <= 0.0f)
    {
        Out.color = failColor;
    }
    else
    {
        Out.color = successColor;
    }
    
    // Tweek to be able to draw pixels evenn if they are to far
    Out.depth = vs.Profondeur.x / 2.0f;

    return Out;
}