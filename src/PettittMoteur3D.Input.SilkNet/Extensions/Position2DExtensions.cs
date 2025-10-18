namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class Position2DExtensions
    {
        public static Position2D FromSilk(this Silk.NET.Input.Position2D position)
        {
            switch (position)
            {
                case Silk.NET.Input.Position2D.Centered: return Position2D.Centered;
                case Silk.NET.Input.Position2D.Up: return Position2D.Up;
                case Silk.NET.Input.Position2D.Down: return Position2D.Down;
                case Silk.NET.Input.Position2D.Left: return Position2D.Left;
                case Silk.NET.Input.Position2D.Right: return Position2D.Right;
                case Silk.NET.Input.Position2D.UpLeft: return Position2D.UpLeft;
                case Silk.NET.Input.Position2D.UpRight: return Position2D.UpRight;
                case Silk.NET.Input.Position2D.DownLeft: return Position2D.DownLeft;
                case Silk.NET.Input.Position2D.DownRight: return Position2D.DownRight;
                default:
                    return Position2D.Centered;
            }
        }

        public static Silk.NET.Input.Position2D ToSilk(this Position2D position)
        {
            switch (position)
            {
                case Position2D.Centered: return Silk.NET.Input.Position2D.Centered;
                case Position2D.Up: return Silk.NET.Input.Position2D.Up;
                case Position2D.Down: return Silk.NET.Input.Position2D.Down;
                case Position2D.Left: return Silk.NET.Input.Position2D.Left;
                case Position2D.Right: return Silk.NET.Input.Position2D.Right;
                case Position2D.UpLeft: return Silk.NET.Input.Position2D.UpLeft;
                case Position2D.UpRight: return Silk.NET.Input.Position2D.UpRight;
                case Position2D.DownLeft: return Silk.NET.Input.Position2D.DownLeft;
                case Position2D.DownRight: return Silk.NET.Input.Position2D.DownRight;
                default:
                    return Silk.NET.Input.Position2D.Centered;
            }
        }
    }
}
