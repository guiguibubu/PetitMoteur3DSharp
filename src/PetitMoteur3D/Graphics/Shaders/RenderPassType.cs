namespace PetitMoteur3D.Graphics.Shaders;

internal enum RenderPassType
{
    ForwardOpac,
    ForwardTransparent,
    DeferredShadingGeometry,
    DeferredShadingGeometryTransparent,
    DeferredShadingLightning,
    DepthTest,
    ShadowMap,
}
