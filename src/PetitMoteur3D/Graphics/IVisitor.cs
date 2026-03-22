namespace PetitMoteur3D.Graphics;

internal interface IVisitor
{
    void Visit(Scene scene);
    void Visit(BaseObjet3D baseObject3D);
    void Visit(SubObjet3D subObjet3D);
}
