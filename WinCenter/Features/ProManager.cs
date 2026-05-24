using System;
using System.IO;
using System.Linq;
using System.Text;

namespace WinCenter.Features
{
    public class ProManager
    {
        private string pathPro;

        // 🔹 costruttore: salva percorso file pro.key
        public ProManager()
        {
            pathPro = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WinCenter",
                "pro.key"
            );
        }

        // 🔹 verifica se Pro è attiva
        public bool IsProActive()
        {
            return File.Exists(pathPro);
        }

        // 🔹 salva codice Pro (codificato base64)
        public void SaveKey(string codice)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(pathPro));

            File.WriteAllText(
                pathPro,
                Convert.ToBase64String(Encoding.UTF8.GetBytes(codice))
            );
        }

        // 🔹 valida codice
        public bool IsValidCode(string codice)
        {
            if (!codice.StartsWith("WIN-"))
                return false;

            string clean = codice.Replace("WIN-", "").Replace("-", "");
            int sum = clean.Sum(c => c);

            return ((sum * 3 + 17) % 20 == 0);
        }
    }
}