using WindowsPhoneSpeedyBlupi;
using static WindowsPhoneSpeedyBlupi.EnvClasses;

static class Program
{
    static void Main()
    {
        Env.init(Impl.MonoGame, Platform.Desktop);
        var game = new WindowsPhoneSpeedyBlupi.Game1();
        game.Run();
    }
}

