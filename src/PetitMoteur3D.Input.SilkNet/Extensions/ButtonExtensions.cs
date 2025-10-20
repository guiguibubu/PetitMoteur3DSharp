namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class ButtonExtensions
    {
        public static Button FromSilk(this Silk.NET.Input.Button button)
        {
            return new Button(button.Name.FromSilk(), button.Index, button.Pressed);
        }

        public static Silk.NET.Input.Button ToSilk(this Button button)
        {
            return new Silk.NET.Input.Button(button.Name.ToSilk(), button.Index, button.Pressed);
        }
    }
}
