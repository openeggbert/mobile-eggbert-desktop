// WindowsPhoneSpeedyBlupi, Version=1.0.0.5, Culture=neutral, PublicKeyToken=6db12cd62dbec439
// WindowsPhoneSpeedyBlupi.Def

using static WindowsPhoneSpeedyBlupi.Xna;

namespace WindowsPhoneSpeedyBlupi
{

    public static class Xna
    {
        public enum Platform
        {
            Desktop,
            Android,
            iOS,
            Web
        }

        public enum XnaImpl
        {
            MonoGame = ProgrammingLanguage.CSharp,
            Fna = ProgrammingLanguage.CSharp,
            Kni = ProgrammingLanguage.CSharp,
            JXNA = ProgrammingLanguage.Java,
            JSXN = ProgrammingLanguage.JavaScript
        }

        public enum ProgrammingLanguage
        {
            CSharp,
            Java,
            JavaScript
        }

    
    }

    public static class Extensions
    {
        public static ProgrammingLanguage getProgrammingLanguage(this XnaImpl xnaImpl)
        {
            return (ProgrammingLanguage)((int)xnaImpl);
        }
        public static bool isDesktop(this Platform platform)
        {
            return platform == Platform.Desktop;
        }
        public static bool isAndroid(this Platform platform)
        {
            return platform == Platform.Android;
        }
        public static bool isIOS(this Platform platform)
        {
            return platform == Platform.iOS;
        }
        public static bool isWeb(this Platform platform)
        {
            return platform == Platform.Web;
        }
    }
}