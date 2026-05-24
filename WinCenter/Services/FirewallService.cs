using System.Diagnostics;

namespace WinCenter.Services
{
    public class FirewallService
    {
        public string GetStatus()
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "netsh";
                p.StartInfo.Arguments = "advfirewall show allprofiles";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;

                p.Start();
                string output = p.StandardOutput.ReadToEnd().ToLower();
                p.WaitForExit();

                int attivo = 0;
                int disattivo = 0;

                foreach (string line in output.Split('\n'))
                {
                    if (line.Contains("stato") || line.Contains("state"))
                    {
                        if (line.Contains("on") || line.Contains("attivo"))
                            attivo++;

                        if (line.Contains("off") || line.Contains("disattivo"))
                            disattivo++;
                    }
                }

                if (attivo > 0 && disattivo > 0)
                    return "Parziale";

                if (attivo > 0)
                    return "Attivo";

                return "Disattivo";
            }
            catch
            {
                return "Errore";
            }
        }
    }
}