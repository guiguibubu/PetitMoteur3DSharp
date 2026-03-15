namespace PetitMoteur3D.Graphics.RenderTechniques;

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
