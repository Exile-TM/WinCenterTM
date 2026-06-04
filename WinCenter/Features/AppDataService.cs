using System;
using System.IO;

namespace WinCenter.Features
{
    public static class AppDataService
    {
        private const string AppDataFolder = "WinCenter";

        public static bool IsPortable
        {
            get { return false; }
        }

        public static string BasePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    AppDataFolder
                );
            }
        }

        public static string WebViewCachePath
        {
            get { return Path.Combine(BasePath, "WebViewCache"); }
        }

        public static string GetPath(string fileName)
        {
            return Path.Combine(BasePath, fileName);
        }

        public static void EnsureBaseFolder()
        {
            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);
        }
    }
}
