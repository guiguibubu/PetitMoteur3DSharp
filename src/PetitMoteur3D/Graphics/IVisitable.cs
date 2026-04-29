namespace PetitMoteur3D.Graphics;

internal interface IVisitable
{
    void Accept(IVisitor visitor);
}

internal interface IVisitable<T> : IVisitable where T : class
{
    void Accept(IVisitor<T> visitor);
}
