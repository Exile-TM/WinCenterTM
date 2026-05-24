using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WinCenter.Services;

namespace WinCenter.Services
{
    public class SystemRepairService
    {
        // 🔹 Esegue SFC + DISM (riparazione completa sistema)
        public async Task RepairSystem()
        {
            // 🔸 SFC (controllo e riparazione file di sistema)
            await Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c sfc /scannow",
                        Verb = "runas",
                        CreateNoWindow = true
                    })?.WaitForExit();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Errore SFC", ex);
                }
            });

            // 🔸 DISM (riparazione immagine Windows)
            await Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c DISM /Online /Cleanup-Image /RestoreHealth",
                        Verb = "runas",
                        CreateNoWindow = true
                    })?.WaitForExit();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Errore DISM", ex);
                }
            });
        }

        // 🔹 Solo diagnosi (senza riparare)
        public async Task DiagnoseSystem()
        {
            await Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c sfc /verifyonly",
                        UseShellExecute = true, // compatibilità Win7
                        CreateNoWindow = true
                    })?.WaitForExit();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Errore diagnosi sistema", ex);
                }
            });
        }

        // 🔹 Pulizia disco (cleanmgr)
        public async Task CleanSystem()
        {
            await Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cleanmgr.exe",
                        Verb = "runas"
                    })?.WaitForExit();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Errore pulizia disco", ex);
                }
            });
        }
    }
}