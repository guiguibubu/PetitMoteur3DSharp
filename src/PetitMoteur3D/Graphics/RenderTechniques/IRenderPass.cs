namespace PetitMoteur3D.Graphics.RenderTechniques;

internal interface IRenderPass
{
    public bool IsEnabled { get; set; }
    public string Name { get; }

    #region Public methods

    void Render(Scene scene);
    void Render(BaseObjet3D objet);
    void Render(SubObjet3D subObjet3D);
    
    #endregion
}
