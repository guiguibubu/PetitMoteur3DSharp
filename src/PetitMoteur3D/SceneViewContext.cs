using System.Numerics;
using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal struct SceneViewContext
{
    public bool UseDebugCam { get; set; }
    public Vector4 GameCameraPos { get; set; }
    public LightShadersParams Light { get; set; }
    public Matrix4x4 MatViewProj { get; set; }
    public Matrix4x4 MatViewProjLight { get; set; }
}
