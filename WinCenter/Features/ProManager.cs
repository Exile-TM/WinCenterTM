using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WinCenter.Features
{
    public class ProManager
    {
        private readonly string pathPro;
        private const string LicensePrefix = "WC2";

        // Firma locale usata per distinguere un file pro.key creato da WinCenter da un file vuoto o scritto a mano.
        // Non sostituisce una licenza server-side, ma evita l'attivazione tramite semplice esistenza del file.
        private const string LicenseSecret = "WinCenter-Pro-License-v2::Informatica-Spiegata-Male::2026";

        public ProManager() : this(AppDataService.GetPath("pro.key"))
        {
        }

        public ProManager(string proKeyPath)
        {
            pathPro = proKeyPath;
        }

        public bool IsProActive()
        {
            try
            {
                if (!File.Exists(pathPro))
                    return false;

                string content = File.ReadAllText(pathPro).Trim();
                if (string.IsNullOrWhiteSpace(content))
                {
                    EliminaChiaveNonValida();
                    return false;
                }

                string decoded = DecodificaBase64(content);
                if (string.IsNullOrWhiteSpace(decoded))
                {
                    EliminaChiaveNonValida();
                    return false;
                }

                if (TryReadSignedLicense(decoded, out string codiceFirmato))
                    return IsValidCode(codiceFirmato) && VerificaFirma(codiceFirmato, EstraiFirma(decoded));

                // Compatibilita con il vecchio pro.key: se conteneva un codice valido, lo accetta e lo risalva firmato.
                string codiceLegacy = decoded.Trim().ToUpperInvariant();
                if (IsValidCode(codiceLegacy))
                {
                    SaveKey(codiceLegacy);
                    return true;
                }

                EliminaChiaveNonValida();
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void SaveKey(string codice)
        {
            if (!IsValidCode(codice))
                throw new InvalidOperationException("Codice Pro non valido.");

            Directory.CreateDirectory(Path.GetDirectoryName(pathPro));

            string normalized = NormalizzaCodice(codice);
            string payload = LicensePrefix + "|" + normalized + "|" + CreaFirma(normalized);
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

            File.WriteAllText(pathPro, encoded);
        }

        public bool IsValidCode(string codice)
        {
            codice = NormalizzaCodice(codice);

            if (IsDeveloperCode(codice))
                return true;

            if (!codice.StartsWith("WIN-"))
                return false;

            string clean = codice.Replace("WIN-", "").Replace("-", "");
            if (clean.Length == 0)
                return false;

            int sum = clean.Sum(c => c);
            return ((sum * 3 + 17) % 20 == 0);
        }

        public bool IsDeveloperCode(string codice)
        {
            byte[] data = { 31, 1, 6, 101, 28, 13, 27, 28, 101, 120, 120, 120, 120 };
            char[] chars = new char[data.Length];

            for (int i = 0; i < data.Length; i++)
                chars[i] = (char)(data[i] ^ 72);

            return string.Equals(NormalizzaCodice(codice), new string(chars), StringComparison.Ordinal);
        }

        private string NormalizzaCodice(string codice)
        {
            return (codice ?? "").Trim().ToUpperInvariant();
        }

        private string DecodificaBase64(string content)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(content));
            }
            catch
            {
                return null;
            }
        }

        private bool TryReadSignedLicense(string decoded, out string codice)
        {
            codice = null;

            string[] parts = decoded.Split('|');
            if (parts.Length != 3 || parts[0] != LicensePrefix)
                return false;

            codice = NormalizzaCodice(parts[1]);
            return !string.IsNullOrWhiteSpace(codice);
        }

        private string EstraiFirma(string decoded)
        {
            string[] parts = decoded.Split('|');
            return parts.Length == 3 ? parts[2] : "";
        }

        private string CreaFirma(string codice)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] data = Encoding.UTF8.GetBytes(LicensePrefix + "|" + NormalizzaCodice(codice) + "|" + LicenseSecret);
                return Convert.ToBase64String(sha.ComputeHash(data));
            }
        }

        private bool VerificaFirma(string codice, string firma)
        {
            return string.Equals(CreaFirma(codice), firma, StringComparison.Ordinal);
        }

        private void EliminaChiaveNonValida()
        {
            try
            {
                if (File.Exists(pathPro))
                    File.Delete(pathPro);
            }
            catch
            {
                // Se non riesce a cancellarla, semplicemente non viene considerata valida.
            }
        }
    }
}
