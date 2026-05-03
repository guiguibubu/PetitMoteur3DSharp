#ifndef TEXTURE_HELPER
#define TEXTURE_HELPER

float3 ReadTextureRGB(Texture2D textureIn, SamplerState samplerTexture, float2 coordTex)
{
    return textureIn.Sample(samplerTexture, coordTex).rgb;
}

#endif
