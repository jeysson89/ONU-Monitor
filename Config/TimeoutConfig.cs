namespace BDCOM.OLT.Manager.Config
{
    internal static class TimeoutConfig
    {
        public static int TelnetTimeout = 10;
        public static int CommandTimeout = 30;
        public static int AuthTimeout = 5;
        public static double CommandDelay = 0.3;
        public static int ReconnectAttempts = 3;
        public static int ReconnectDelay = 5;
    }
}