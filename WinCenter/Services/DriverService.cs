using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WinCenter.Services;
using System.Threading.Tasks;

namespace WinCenter.Services
{
    public class DriverService
    {
        // 🔹 Installa driver da cartella (pnputil)
        public async Task<bool> InstallDriver(string driverPath)
        {
            try
            {
                bool successo = true;

                await Task.Run(() =>
                {
                    try
                    {
                        var proc = Process.Start(new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c pnputil /add-driver \"{driverPath}\\*.inf\" /subdirs /install",
                            Verb = "runas",
                            UseShellExecute = true,
                            CreateNoWindow = true
                        });

                        proc?.WaitForExit();

                        if (proc == null)
                            successo = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Errore installazione driver", ex);
                        successo = false;
                    }
                });

                return successo;
            }
            catch
            {
                return false;
            }
        }

        // 🔹 Backup driver
        public async Task<bool> BackupDrivers(string path)
        {
            try
            {
                Directory.CreateDirectory(path);

                bool successo = true;

                await Task.Run(() =>
                {
                    try
                    {
                        var proc = Process.Start(new ProcessStartInfo
                        {
                            FileName = "dism.exe",
                            Arguments = $"/online /export-driver /destination:\"{path}\"",
                            Verb = "runas",
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        });

                        proc?.WaitForExit();

                        if (proc == null || proc.ExitCode != 0)
                            successo = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Errore backup driver", ex);
                        successo = false;
                    }
                });

                // 🔹 controllo reale: esistono file .inf?
                if (!Directory.EnumerateFiles(path, "*.inf", SearchOption.AllDirectories).Any())
                    successo = false;

                return successo;
            }
            catch
            {
                return false;
            }
        }

        // 🔹 Restore driver
        public async Task<(bool successo, bool giaPresenti)> RestoreDrivers(string path)
        {
            bool successo = true;
            bool giaPresenti = false;

            await Task.Run(() =>
            {
                try
                {
                    var proc = new Process();

                    proc.StartInfo.FileName = "cmd.exe";
                    proc.StartInfo.Arguments = $"/c pnputil /add-driver \"{path}\\*.inf\" /subdirs /install";
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;

                    proc.Start();

                    string output = proc.StandardOutput.ReadToEnd().ToLower();

                    proc.WaitForExit();

                    if (output.Contains("errore") || output.Contains("failed"))
                        successo = false;

                    if (!output.Contains("install"))
                        giaPresenti = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError("Errore restore driver", ex);
                    successo = false;
                }
            });

            return (successo, giaPresenti);
        }
    }
}