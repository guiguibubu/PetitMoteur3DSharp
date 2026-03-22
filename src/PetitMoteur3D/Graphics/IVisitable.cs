namespace PetitMoteur3D.Graphics;

internal interface IVisitable
{
    void Accept(IVisitor visitor);
}
