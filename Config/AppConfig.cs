using System.IO;

namespace BDCOM.OLT.Manager.Config
{
    internal static class AppConfig
    {
        public static string AppName = "BDCOM OLT Manager";
        public static string Version = "16.5";
        public static string ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".olt_manager_pro");
        public static string DevicesFile => Path.Combine(ConfigDir, "devices.json");
        public static string LogFile => Path.Combine(ConfigDir, "app.log");

        public static bool AutoConnect = true;
        public static bool AutoReconnect = true;
        public static int MaxDevicesPerRow = 8;

        public static void EnsureDirs()
        {
            Directory.CreateDirectory(ConfigDir);
        }
    }
}