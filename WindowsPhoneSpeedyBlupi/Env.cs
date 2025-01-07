using System;
using static WindowsPhoneSpeedyBlupi.Xna;

namespace WindowsPhoneSpeedyBlupi
{
    public static class Env
    {
        public static bool DETAILED_DEBUGGING { get; set; }

        public static Platform PLATFORM { get; private set; }
        public static XnaImpl XNA_IMPL { get; private set; }
        public static bool INITIALIZED { get; private set; }

        public static void init(XnaImpl xnaImpl, Platform platformIn)
        {
            if(INITIALIZED)
            {
                throw new Exception("Env was already initialized. Cannot call the init method again.");
            }
            XNA_IMPL = xnaImpl;
            PLATFORM = platformIn;
            INITIALIZED = true;
        }
    }
}
