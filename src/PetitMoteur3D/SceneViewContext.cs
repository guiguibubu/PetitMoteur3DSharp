using System.Numerics;
using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal ref struct SceneViewContext
{
    public bool ShowShadow { get; set; }
    public bool UseDebugCam { get; set; }
    public Vector4 GameCameraPos { get; set; }
    public LightShadersParams Light { get; set; }
    public Matrix4x4 MatViewProj { get; set; }
    public Matrix4x4 MatViewProjLight { get; set; }
    public ShadowMap ShadowMap { get; set; }
}
