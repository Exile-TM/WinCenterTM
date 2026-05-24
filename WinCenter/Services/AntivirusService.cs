using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using WinCenter.Services;

namespace WinCenter.Services
{
    public class AntivirusService
    {
        // 🔹 Restituisce stato antivirus + nome
        public (string stato, string nome) GetInfo()
        {
            try
            {
                // 🔥 metodo principale: PowerShell (Defender)
                Process p = new Process();

                p.StartInfo.FileName = "powershell";
                p.StartInfo.Arguments = "Get-MpComputerStatus | Select-Object -ExpandProperty RealTimeProtectionEnabled";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;

                p.Start();

                string output = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit();

                // 🔹 se True → attivo
                if (output.ToLower().Contains("true"))
                    return ("Attivo", "Windows Defender");
                else
                    return ("Disattivo", "Windows Defender");
            }
            catch (Exception ex)
            {
                // 🔹 log errore
                Logger.LogError("Errore rilevamento antivirus (PowerShell)", ex);

                // fallback vecchio metodo
                try
                {
                    var searcher = new ManagementObjectSearcher(
                        @"root\SecurityCenter2",
                        "SELECT * FROM AntiVirusProduct"
                    );

                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string nome = obj["displayName"]?.ToString();

                        // anche se non abbiamo stato preciso → consideriamo attivo
                        return ("Attivo", nome);
                    }
                }
                catch { }

                // ❌ nessun antivirus trovato
                return ("Non disponibile", "Nessun antivirus rilevato");
            }
        }

        // 🔹 Avvia scansione Defender
        public void StartScan()
        {
            try
            {
                string defenderPath = null;

                // 🔹 Metodo 1: percorso classico
                string path1 = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Windows Defender",
                    "MpCmdRun.exe"
                );

                if (File.Exists(path1))
                {
                    defenderPath = path1;
                }

                // 🔹 Metodo 2: nuove versioni Defender
                if (defenderPath == null)
                {
                    string basePath = @"C:\ProgramData\Microsoft\Windows Defender\Platform";

                    if (Directory.Exists(basePath))
                    {
                        var dir = new DirectoryInfo(basePath);

                        var latest = dir.GetDirectories()
                                        .OrderByDescending(d => d.Name)
                                        .FirstOrDefault();

                        if (latest != null)
                        {
                            string path2 = Path.Combine(latest.FullName, "MpCmdRun.exe");

                            if (File.Exists(path2))
                                defenderPath = path2;
                        }
                    }
                }

                // 🔹 Se trovato → avvia scansione
                if (defenderPath != null)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = defenderPath,
                        Arguments = "-Scan -ScanType 1",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Verb = "runas"
                    });

                    return;
                }

                // 🔹 fallback: apre download scanner Microsoft
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.microsoft.com/it-it/download/details.aspx?id=9905",
                    UseShellExecute = true
                });
            }
            catch
            {
                throw new Exception("Errore avvio scansione");
            }
        }
    }
}