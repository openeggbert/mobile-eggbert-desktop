using WindowsPhoneSpeedyBlupi;
using static WindowsPhoneSpeedyBlupi.Xna;

static class Program
{
    static void Main()
    {
        Env.init(XnaImpl.MonoGame, Platform.Desktop);
        var game = new WindowsPhoneSpeedyBlupi.Game1();
        game.Run();
    }
}

