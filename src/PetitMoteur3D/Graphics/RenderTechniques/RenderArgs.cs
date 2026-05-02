namespace PetitMoteur3D.Graphics.RenderTechniques;

internal struct RenderArgs
{
    public SceneViewContext SceneContext;
    public ObjectViewContext ObjectContext;

    public RenderArgs()
    {
        SceneContext = new SceneViewContext();
        ObjectContext = new ObjectViewContext();
    }
}
