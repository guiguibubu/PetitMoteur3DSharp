namespace PetitMoteur3D.Graphics.RenderTechniques;

internal class RenderTechnique
{
    private IRenderPass[] _passes;

    public RenderTechnique(params IRenderPass[] passes)
    {
        _passes = passes;
    }

    public void Render(Scene scene)
    {
        foreach (IRenderPass pass in _passes)
        {
            if (pass.IsEnabled)
            {
                pass.Render(scene);
            }
        }
    }
}
