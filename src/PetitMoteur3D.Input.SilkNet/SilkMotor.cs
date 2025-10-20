namespace PetitMoteur3D.Input.SilkNet;

internal class SilkMotor : IMotor
{
    private readonly Silk.NET.Input.IMotor _silkMotor;

    public SilkMotor(Silk.NET.Input.IMotor silkMotor)
    {
        _silkMotor = silkMotor;
    }
    public int Index => _silkMotor.Index;

    public float Speed { get => _silkMotor.Speed; set => _silkMotor.Speed = value; }
}
