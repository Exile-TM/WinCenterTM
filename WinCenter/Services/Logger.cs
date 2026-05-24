using System;
using System.IO;

namespace WinCenter.Services
{
    public static class Logger
    {
        // 🔹 percorso file log
        private static string logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinCenter",
            "log.txt"
        );

        // 🔹 scrive messaggio nel log
        public static void Log(string message)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));

                File.AppendAllText(
                    logPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n"
                );
            }
            catch
            {
                // ❌ se fallisce, non blocca il programma
            }
        }

        // 🔹 scrive errore
        public static void LogError(string message, Exception ex)
        {
            Log($"ERRORE: {message} | {ex.Message}");
        }
    }
}