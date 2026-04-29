namespace PetitMoteur3D.Graphics;

internal interface IVisitor
{
    void Visit(IVisitable visitable);
}

internal interface IVisitor<T> where T : class
{
    void Visit(T visitable);
}
