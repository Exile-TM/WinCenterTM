using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WinCenter.Services; // richiama la cartella Services

namespace WinCenter
{
    public partial class Form1 : Form
    {
        // Versione Pro attiva
        string pathPro = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinCenter",
            "pro.key"
        );
        // 🔹 file accettazione Terms
        string pathTerms = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinCenter",
            "terms.accepted"
        );

        // 🔹 servizi globali
        FirewallService firewallService = new FirewallService();
        AntivirusService antivirusService = new AntivirusService();
        DriverService driverService = new DriverService();
        SystemRepairService repairService = new SystemRepairService();
        WinCenter.Features.ProManager proManager = new WinCenter.Features.ProManager();

        public Form1()
        {
            InitializeComponent();
            webView21.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            this.Text = "WinCenter™ Online Edition v1.2 - Informatica Spiegata Male";
            this.Icon = new Icon(
                Path.Combine(Application.StartupPath, "Contenuti", "Immagini", "Icone", "WinCenter.ico")
            );
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(650, 335);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // ✅ EVENTO INIT WEBVIEW
        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                webView21.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            }
        }

        // ✅ RICEZIONE MESSAGGI (Firewall + Antivirus)
        private async void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string msg = e.TryGetWebMessageAsString();
            // ===== COMPATIBILITA PROGRAMMI =====
            if (msg == "get_compat_info")
            {
                string pcJson = GetCompatibilityInfoJson();

                await webView21.CoreWebView2.ExecuteScriptAsync(
                    "if(window.setCompatibilityInfo){ setCompatibilityInfo(" + pcJson + "); }"
                );

                return;
            }

            // ===== TERMS =====
            if (msg == "accept_terms")
            {
                // 🔹 crea cartella se non esiste
                Directory.CreateDirectory(Path.GetDirectoryName(pathTerms));

                // 🔹 salva file accettazione
                File.WriteAllText(pathTerms, "accepted");

                // 🔹 vai alla home
                webView21.Source = new Uri(Path.Combine(Application.StartupPath, "Avvio", "main.html"));
                return;
            }
            // ===== SUPPORTO =====
            if (msg == "supporto")
            {
                // 🔹 recupero info PC (riuso logica tua)
                string os = "Windows";
                var osSearch = new System.Management.ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
                foreach (var item in osSearch.Get())
                    os = item["Caption"].ToString();

                string cpu = "";
                var cpuSearch = new System.Management.ManagementObjectSearcher("select Name from Win32_Processor");
                foreach (var item in cpuSearch.Get())
                    cpu = item["Name"].ToString();

                cpu = cpu.Replace("(R)", "").Replace("(TM)", "").Replace("CPU", "").Trim();

                var ramSearch = new System.Management.ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
                ulong ramKb = 0;
                foreach (var item in ramSearch.Get())
                    ramKb = (ulong)item["TotalVisibleMemorySize"];

                string ram = (ramKb / 1024 / 1024) + " GB";

                // 🔹 costruzione body
                string body =
                    "=== WinCenter Support ===%0A%0A" +

                    "🖥️ Sistema/System:%0A" +
                    "OS: " + os + "%0A" +
                    "CPU: " + cpu + "%0A" +
                    "RAM: " + ram + "%0A%0A" +

                    "🇮🇹 ITALIANO:%0A" +
                    "Descrizione del problema:%0A- %0A%0A" +

                    "🇬🇧 ENGLISH:%0A" +
                    "Problem description:%0A- %0A%0A" +

                    "----------------------------%0A" +
                    "Grazie per il supporto ❤️%0A" +
                    "Thanks for supporting WinCenter!";

                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://mail.google.com/mail/?view=cm&fs=1" +
                               "&to=support.wincenter@gmail.com" +
                               "&su=Supporto WinCenter / WinCenter Support" +
                               "&body=" + body,
                    UseShellExecute = true
                });

                return;
            }
            // ===== FIREWALL =====
            if (msg == "check_firewall")
            {
                string stato = firewallService.GetStatus();

                await webView21.CoreWebView2.ExecuteScriptAsync(
                    "if(window.setFirewallStatus){ setFirewallStatus('" + stato + "'); }"
                );
            }
            if (msg == "avvia_scanner")
            {

                // 🔹 avvio scansione
                antivirusService.StartScan();

                // ⏱️ gestione UI dopo 30 secondi (identica a prima)
                _ = Task.Run(async () =>
                {
                    await Task.Delay(30000);
                    await webView21.CoreWebView2.ExecuteScriptAsync("stopScanUI()");

                    // 🔹 riprendo info aggiornate
                    var info = antivirusService.GetInfo();
                    await webView21.CoreWebView2.ExecuteScriptAsync(
                        $"updateAntivirusStatus('{info.stato}', '{info.nome}')"
                    );
                });
            }
            if (msg == "check_antivirus")
            {
                // 🔹 prendo info (stato + nome)
                var info = antivirusService.GetInfo();

                // 🔹 aggiorno UI
                await webView21.CoreWebView2.ExecuteScriptAsync(
                    $"updateAntivirusStatus('{info.stato}', '{info.nome}')"
                );
            }
  
            // ===== INSTALLAZIONE DRIVER =====
            if (msg.StartsWith("driver_"))
            {
                string tipo = msg.Replace("driver_", "");
                string basePath = Path.Combine(
                    Application.StartupPath,
                    "Contenuti",
                    "Driver"
                );
                string driverPath = "";
                switch (tipo)
                {
                    case "realtek_lan":
                        driverPath = Path.Combine(basePath, "Realtek");
                        break;

                    case "intel_lan":
                        driverPath = Path.Combine(basePath, "Intel");
                        break;

                    case "intel_wifi":
                        driverPath = Path.Combine(basePath, "Intel");
                        break;

                    case "realtek_wifi":
                        driverPath = Path.Combine(basePath, "Realtek");
                        break;
                }
                if (!Directory.Exists(driverPath))
                {
                    await webView21.ExecuteScriptAsync("mostraPopup('driver_not_found')");
                    return;
                }
                await webView21.ExecuteScriptAsync("mostraProgress()");
                // 🔹 progress fluido iniziale
                _ = SmoothProgress(0, 50, 25);

                // 🔹 eseguo installazione
                bool successo = await driverService.InstallDriver(driverPath);

                // 🔹 completamento fluido
                await SmoothProgress(50, 100, 15);

                await webView21.ExecuteScriptAsync("nascondiProgress()");

                if (successo)
                {
                    await webView21.ExecuteScriptAsync(
                        "mostraPopup('driver_install_success')"
                    );
                }
                else
                {
                    await webView21.ExecuteScriptAsync(
                        "mostraPopup('driver_install_error')"
                    );
                }
                return;
            }

            // ===== WEI =====
            if (msg == "wei_riesegui")
            {
                string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string pathSysnative = Path.Combine(winDir, "Sysnative", "winsat.exe");
                string pathSystem32 = Path.Combine(winDir, "System32", "winsat.exe");
                string winsatPath = null;

                if (File.Exists(pathSysnative))
                    winsatPath = pathSysnative;
                else if (File.Exists(pathSystem32))
                    winsatPath = pathSystem32;

                if (winsatPath == null)
                {
                    await ShowLangMessage("WEI_NOTFOUND");
                    return;
                }

                try
                {
                    var proc = Process.Start(new ProcessStartInfo
                    {
                        FileName = winsatPath,
                        Arguments = "formal",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });

                    if (proc != null)
                    {
                        await Task.Run(() => proc.WaitForExit());
                    }

                    await CaricaWei();
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    await ShowLangMessage("WEI_ADMIN");
                }
                return;
            }

            if (msg == "wei_refresh")
            {
                await CaricaWei();
                return;
            }

            // ===== CHIUSURA =====
            if (msg == "chiudi")
            {
                Application.Exit();
                return;
            }

            // 🔹 Apri lettore musicale
            if (msg == "apri_lettore")
            {
                Form player = new Form();
                player.Icon = new Icon(Path.Combine(
                    Application.StartupPath,
                    "Contenuti", "Immagini", "Icone", "music.ico"
                ));
                player.Text = "Lettore musicale"; // verrà sovrascritto
                player.Size = new Size(300, 135);
                player.FormBorderStyle = FormBorderStyle.FixedSingle;
                player.MaximizeBox = false;
                player.MinimizeBox = false;
                player.StartPosition = FormStartPosition.CenterScreen;

                WebView2 web = new WebView2();
                web.Dock = DockStyle.Fill;
                player.Controls.Add(web);

                player.Show();

                await web.EnsureCoreWebView2Async();

                // 🔹 Recupera lingua
                string lang = "it";

                try
                {
                    var result = await webView21.CoreWebView2.ExecuteScriptAsync("localStorage.getItem('lang')");
                    lang = result.Replace("\"", "");

                    if (string.IsNullOrEmpty(lang) || lang == "null")
                        lang = "it";
                }
                catch
                {
                    lang = "it";
                }

                // 🔹 Path + lingua
                string filePath = Path.Combine(
                    Application.StartupPath,
                    "Contenuti", "HTML", "RapidMenu", "MediaPlayer", "musica.html"
                );

                var uri = new UriBuilder(new Uri(filePath));
                uri.Query = "lang=" + lang;

                web.Source = uri.Uri;

                // 🔥 titolo + autoplay
                web.CoreWebView2.NavigationCompleted += async (s, ev) =>
                {
                    var title = await web.ExecuteScriptAsync("document.title");
                    title = title.Replace("\"", "");

                    player.Text = title;

                    await web.ExecuteScriptAsync("document.getElementById('player').play()");
                };
            }

            // 🔹 Apri info
            if (msg == "apri_info")
            {
                Form info = new Form();
                info.Text = "Informazioni"; // verrà sovrascritto

                info.ClientSize = new Size(320, 500);
                info.FormBorderStyle = FormBorderStyle.FixedSingle;
                info.MaximizeBox = false;
                info.MinimizeBox = false;
                info.StartPosition = FormStartPosition.CenterScreen;

                info.Icon = new Icon(Path.Combine(
                    Application.StartupPath,
                    "Contenuti", "Immagini", "Icone", "info.ico"
                ));

                var web = new Microsoft.Web.WebView2.WinForms.WebView2();
                web.Dock = DockStyle.Fill;
                info.Controls.Add(web);

                info.Show();

                await web.EnsureCoreWebView2Async();

                // 🔹 Recupera lingua
                string lang = "it";

                try
                {
                    var result = await webView21.CoreWebView2.ExecuteScriptAsync("localStorage.getItem('lang')");
                    lang = result.Replace("\"", "");

                    if (string.IsNullOrEmpty(lang) || lang == "null")
                        lang = "it";
                }
                catch
                {
                    lang = "it";
                }

                // 🔹 Path + lingua
                string filePath = Path.Combine(
                    Application.StartupPath,
                    "Contenuti", "HTML", "RapidMenu", "Info", "info.html"
                );

                var uri = new UriBuilder(new Uri(filePath));
                uri.Query = "lang=" + lang;

                web.Source = uri.Uri;

                // 🔥 titolo dinamico da JS
                web.CoreWebView2.NavigationCompleted += async (s, ev) =>
                {
                    var title = await web.ExecuteScriptAsync("document.title");
                    title = title.Replace("\"", "");

                    info.Text = title;
                };
            }

            // ===== PRO =====
            if (msg.StartsWith("PRO:"))
            {
                string codice = msg.Replace("PRO:", "").Trim().ToUpper();

                if (codice == "WIN-96KF-8CT9")
                {
                    await webView21.ExecuteScriptAsync("mostraPopupLang('MSG_EASTER')");
                    return;
                }

                if (codice == "WIN-TEST-0000")
                {
                    proManager.SaveKey(codice);

                    await webView21.ExecuteScriptAsync(@"
mostraPopupLang('MSG_TEST');
setTimeout(function(){
    window.location.href='../../../Avvio/main.html';
}, 1000);
");
                    return;
                }

                if (!codice.StartsWith("WIN-"))
                {
                    await webView21.ExecuteScriptAsync("mostraPopupLang('MSG_FORMAT')");
                    return;
                }

                if (string.IsNullOrEmpty(codice))
                {
                    await webView21.ExecuteScriptAsync("mostraPopupLang('MSG_EMPTY')");
                    return;
                }

                if (proManager.IsValidCode(codice))
                {
                    proManager.SaveKey(codice);

                    await webView21.ExecuteScriptAsync(@"
mostraPopupLang('MSG_SUCCESS');
setTimeout(function(){
    window.location.href='../../../Avvio/main.html';
}, 3000);
");
                }
                else
                {
                    await webView21.ExecuteScriptAsync("mostraPopupLang('MSG_INVALID')");
                }
            }
            // ===== RIPARAZIONE SISTEMA =====
            if (msg == "ripara")
            {
                await EseguiRiparazione();
                return;
            }
            if (msg == "diagnosi")
            {
                await EseguiDiagnosi();
                return;
            }
            if (msg == "pulizia")
            {
                await EseguiPulizia();
                return;
            }

            // ===== BACKUP DRIVER =====
            if (msg == "backup_driver")
            {
                await webView21.ExecuteScriptAsync("mostraProgress()");
                await webView21.ExecuteScriptAsync("aggiornaProgress(5, 'backup_progress_init')");

                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "BackupDriver"
                );
                Directory.CreateDirectory(path);
                bool finito = false;

                // 🔄 Loop progressivo (thread UI-safe)
                var progressLoop = Task.Run(async () =>
                {
                    int p = 10;
                    while (!finito)
                    {
                        await Task.Delay(600);

                        if (p < 90)
                        {
                            p += (p < 50) ? 3 : 2;

                            // 👇 TORNA nel thread principale
                            this.Invoke(new Action(async () =>
                            {
                                string testo = p < 30 ? "backup_progress_prepare" :
                                               p < 60 ? "backup_progress_collect" :
                                               p < 85 ? "backup_progress_export" :
                                               "backup_progress_almost_done";

                                await webView21.ExecuteScriptAsync(
                                    $"aggiornaProgress({p}, '{testo}')"
                                );
                            }));
                        }
                    }
                });

                // 🔹 eseguo backup driver
                bool successo = await driverService.BackupDrivers(path);

                if (successo)
                {
                    await webView21.ExecuteScriptAsync("aggiornaProgress(100, 'backup_progress_finalize')");
                    await Task.Delay(400);
                    await webView21.ExecuteScriptAsync("nascondiProgress()");
                    await webView21.ExecuteScriptAsync("mostraPopup('backup_completed')");
                }
                else
                {
                    await webView21.ExecuteScriptAsync("nascondiProgress()");
                    await webView21.ExecuteScriptAsync("mostraPopup('backup_error')");
                }

                return;
            }

            // ===== RESTORE DRIVER =====
            if (msg == "restore_driver")
            {
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "BackupDriver"
                );

                // 🔴 controllo cartella
                if (!Directory.Exists(path))
                {
                    await webView21.ExecuteScriptAsync("mostraPopup('backup_folder_missing')");
                    return;
                }

                // 🔴 controllo file INF
                if (!Directory.EnumerateFiles(path, "*.inf", SearchOption.AllDirectories).Any())
                {
                    await webView21.ExecuteScriptAsync("mostraPopup('backup_no_valid_driver')");
                    return;
                }

                await webView21.ExecuteScriptAsync("mostraProgress()");
                await webView21.ExecuteScriptAsync("aggiornaProgress(10, 'backup_progress_prepare')");

                // 🔹 eseguo ripristino driver
                var result = await driverService.RestoreDrivers(path);

                // 🔹 prendo risultati
                bool successo = result.successo;
                bool giaPresenti = result.giaPresenti;

                await webView21.ExecuteScriptAsync("aggiornaProgress(90, 'backup_progress_finalize')");
                await Task.Delay(500);

                await webView21.ExecuteScriptAsync("aggiornaProgress(100, 'backup_progress_completed')");
                await Task.Delay(400);

                await webView21.ExecuteScriptAsync("nascondiProgress()");

                if (successo)
                {
                    string msgFinale;

                    if (giaPresenti)
                    {
                        msgFinale = "restore_driver_already_present";
                    }
                    else
                    {
                        msgFinale = "restore_completed";
                    }

                    await webView21.ExecuteScriptAsync($"mostraPopup('{msgFinale}')");
                }
                else
                {
                    await webView21.ExecuteScriptAsync(
                        "mostraPopup('restore_error')"
                    );
                }

                return;
            }
            // ===== APRI CARTELLA DRIVER =====
            if (msg == "apri_driver_folder")
            {
                string path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "BackupDriver"
                );

                if (!Directory.Exists(path) ||
                    !Directory.EnumerateFiles(path, "*.inf", SearchOption.AllDirectories).Any())
                {
                    await webView21.ExecuteScriptAsync(
                        "mostraPopup('backup_not_found')"
                    );
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });

                return;
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Version v = Environment.OSVersion.Version;
            if (v.Major < 6 || (v.Major == 6 && v.Minor == 0))
            {
                MessageBox.Show("Richiede almeno Windows 7 SP1", "Errore");
                Application.Exit();
                return;
            }

            string userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WinCenter", "WebViewCache"
            );

            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webView21.EnsureCoreWebView2Async(env);

            // 🔹 quando la pagina è caricata
            webView21.CoreWebView2.NavigationCompleted += async (s, ev) =>
            {
                try
                {
                    bool termsOk = File.Exists(pathTerms);

                    await webView21.CoreWebView2.ExecuteScriptAsync(
                        $"window.termsAccepted = {(termsOk ? "true" : "false")};"
                    );
                }
                catch { }
            };

            // 🔥 DOM LOADED
            webView21.CoreWebView2.DOMContentLoaded += async (s, ev) =>
            {
                string url = webView21.Source.ToString();

                // ===== WEI =====
                if (url.Contains("wei.html"))
                {
                    await CaricaWei();
                    return;
                }

                // ===== MAIN =====
                if (!url.Contains("main.html") &&
                    !url.Contains("riparazione.html") &&
                    !url.Contains("driver.html"))
                    return;

                // ===== COMPATIBILITÀ AVANZATE ====

                // Windows 8+ = 6.2+
                bool isWin8Plus = (v.Major > 6) || (v.Major == 6 && v.Minor >= 2);
                bool isWin7 = (v.Major == 6 && v.Minor == 1);

                // 👉 prima passo info al JS
                await webView21.ExecuteScriptAsync(
                    $"window.isWin7 = {(isWin7 ? "true" : "false")};"
                );

                // 👉 poi gestisco UI
                if (isWin8Plus)
                {
                    await webView21.ExecuteScriptAsync("abilitaAvanzate()");
                }
                else
                {
                    await webView21.ExecuteScriptAsync("disabilitaAvanzate()");
                }
                // 👉 DISABILITA BOTTONI SU WIN7
                if (isWin7)
                {
                    await webView21.ExecuteScriptAsync(@"
        let b1 = document.getElementById('btnBackup');
        let b2 = document.getElementById('btnRestore');

        if(b1){
            b1.disabled = true;
            b1.style.background = 'gray';
            b1.style.cursor = 'not-allowed';
        }

        if(b2){
            b2.disabled = true;
            b2.style.background = 'gray';
            b2.style.cursor = 'not-allowed';
        }
    ");
                }

                // 🔹 uso ProManager
                var proManager = new WinCenter.Features.ProManager();

                // 🔹 controllo se Pro attiva
                bool isPro = proManager.IsProActive();

                if (isPro)
                {
                    await webView21.ExecuteScriptAsync(@"
        if(window.sbloccaAvanzate){
            sbloccaAvanzate();
        }
    ");
                }

                await webView21.ExecuteScriptAsync(
                    "window.versioneProAttiva = " + (isPro ? "true" : "false") + ";"
                );
                await webView21.ExecuteScriptAsync("aggiornaOrologio();");

                string os = "Windows";
                var osSearch = new System.Management.ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
                foreach (var item in osSearch.Get())
                    os = item["Caption"].ToString();

                string cpu = "";
                var cpuSearch = new System.Management.ManagementObjectSearcher("select Name from Win32_Processor");
                foreach (var item in cpuSearch.Get())
                    cpu = item["Name"].ToString();

                cpu = cpu.Replace("(R)", "").Replace("(TM)", "").Replace("CPU", "").Trim();

                var ramSearch = new System.Management.ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
                ulong ramKb = 0;
                foreach (var item in ramSearch.Get())
                    ramKb = (ulong)item["TotalVisibleMemorySize"];

                string ram = (ramKb / 1024 / 1024) + " GB";

                string js = $@"
(function(){{
    var el = document.getElementById('info-pc');
    if(!el) return;

    el.innerHTML =
        'OS: {os.Replace("'", "")}<br>' +
        'CPU: {cpu.Replace("'", "")}<br>' +
        'RAM: {ram}';
}})();
";

                await webView21.CoreWebView2.ExecuteScriptAsync(js);

                try
                {
                    await webView21.CoreWebView2.ExecuteScriptAsync("mostraScan()");

                    // 🔹 STEP 1
                    await webView21.CoreWebView2.ExecuteScriptAsync("aggiornaScan(10, 'driver_scan_init')");
                    await Task.Delay(500);

                    await webView21.CoreWebView2.ExecuteScriptAsync("aggiornaScan(30, 'driver_scan_detect_cards')");
                    await Task.Delay(700);

                    string driverConsigliato = "realtek_lan";

                    var searcher = new ManagementObjectSearcher(
                        "SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter=True"
                    );

                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string nome = obj["Name"]?.ToString().ToLower();
                        if (nome == null) continue;

                        if (nome.Contains("intel"))
                        {
                            driverConsigliato = "intel_lan";
                            break;
                        }

                        if (nome.Contains("realtek"))
                        {
                            driverConsigliato = "realtek_lan";
                        }
                    }

                    // 🔹 STEP 2
                    await webView21.CoreWebView2.ExecuteScriptAsync("aggiornaScan(70, 'driver_scan_analyze_driver')");
                    await Task.Delay(700);

                    // 🔹 STEP 3
                    await webView21.CoreWebView2.ExecuteScriptAsync("aggiornaScan(90, 'driver_scan_prepare')");
                    await Task.Delay(500);

                    // 🔹 evidenzia PRIMA di chiudere (più naturale)
                    await webView21.CoreWebView2.ExecuteScriptAsync(
                        $"if(window.evidenziaDriver) evidenziaDriver('{driverConsigliato}');"
                    );

                    await webView21.CoreWebView2.ExecuteScriptAsync("aggiornaScan(100, 'driver_scan_completed')");
                    await Task.Delay(600);

                    // 🔹 chiudi popup
                    await webView21.CoreWebView2.ExecuteScriptAsync("nascondiScan()");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Errore UI/WebView scan driver", ex);
                }
            };

            // webView21.CoreWebView2.WebMessageReceived += WebMessageReceived;

            webView21.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;
            webView21.CoreWebView2.Settings.IsStatusBarEnabled = false;

            // 🔹 controlla se Terms già accettati
            string pathIndex = Path.Combine(Application.StartupPath, "Avvio", "index.html");
            webView21.Source = new Uri(pathIndex);
        }

        // ===== FUNZIONE WEI - INDICE PRESTAZIONI =====
        async Task CaricaWei()
        {
            string path = @"C:\Windows\Performance\WinSAT\DataStore";

            var file = Directory.GetFiles(path, "*Formal.Assessment*.xml")
                                .OrderByDescending(f => File.GetLastWriteTime(f))
                                .FirstOrDefault();

            if (file == null) return;

            var doc = XDocument.Load(file);
            var winSPR = doc.Descendants("WinSPR").FirstOrDefault();

            if (winSPR == null) return;

            string json = "{"
                + $"cpu:'{winSPR.Element("CpuScore")?.Value}',"
                + $"ram:'{winSPR.Element("MemoryScore")?.Value}',"
                + $"gpu:'{winSPR.Element("GraphicsScore")?.Value}',"
                + $"gaming:'{winSPR.Element("GamingScore")?.Value}',"
                + $"disk:'{winSPR.Element("DiskScore")?.Value}',"
                + $"base:'{winSPR.Element("SystemScore")?.Value}'"
                + "}";

            await webView21.ExecuteScriptAsync($"setData({json})");
            string hw = GetHardwareInfo();
            await webView21.ExecuteScriptAsync($"setHardware({hw})");
        }
        string GetHardwareInfo()
        {
            string cpu = "", gpu = "", ram = "", vram = "", disk = "";

            try
            {
                // CPU
                using (var searcher = new ManagementObjectSearcher("select Name from Win32_Processor"))
                {
                    foreach (var item in searcher.Get())
                        cpu = item["Name"]?.ToString();
                }

                // GPU + VRAM
                using (var searcher = new ManagementObjectSearcher("select Name, AdapterRAM from Win32_VideoController"))
                {
                    foreach (var item in searcher.Get())
                    {
                        gpu = item["Name"]?.ToString();
                        if (item["AdapterRAM"] != null)
                            vram = (Convert.ToDouble(item["AdapterRAM"]) / 1024 / 1024 / 1024).ToString("0.0") + " GB";
                    }
                }

                // RAM totale
                using (var searcher = new ManagementObjectSearcher("select TotalPhysicalMemory from Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        double bytes = Convert.ToDouble(item["TotalPhysicalMemory"]);
                        ram = (bytes / 1024 / 1024 / 1024).ToString("0") + " GB";
                    }
                }

                // Disco C:
                var drive = new DriveInfo("C");
                disk = (drive.TotalSize / 1024 / 1024 / 1024).ToString() + " GB";
            }
            catch { }

            return "{"
                + $"cpuName:'{cpu}',"
                + $"gpuName:'{gpu}',"
                + $"ramTotal:'{ram}',"
                + $"vramTotal:'{vram}',"
                + $"diskTotal:'{disk}'"
                + "}";
        }

        string GetCompatibilityInfoJson()
        {
            string osName = "Windows";
            string osVersion = "";
            string cpuName = "";
            bool cpu64 = Environment.Is64BitOperatingSystem;
            double ramGb = 0;
            double diskFreeGb = 0;

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption, Version, OSArchitecture FROM Win32_OperatingSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        osName = item["Caption"]?.ToString() ?? osName;
                        osVersion = item["Version"]?.ToString() ?? "";

                        string arch = item["OSArchitecture"]?.ToString() ?? "";
                        if (arch.Contains("64"))
                            cpu64 = true;
                    }
                }

                using (var searcher = new ManagementObjectSearcher("select Name, AddressWidth from Win32_Processor"))
                {
                    foreach (var item in searcher.Get())
                    {
                        cpuName = item["Name"]?.ToString() ?? "";

                        if (item["AddressWidth"] != null)
                            cpu64 = Convert.ToInt32(item["AddressWidth"]) >= 64;
                    }
                }

                using (var searcher = new ManagementObjectSearcher("select TotalPhysicalMemory from Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        double bytes = Convert.ToDouble(item["TotalPhysicalMemory"]);
                        ramGb = bytes / 1024 / 1024 / 1024;
                    }
                }

                var drive = new DriveInfo("C");
                diskFreeGb = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
            }
            catch { }

            int osMajor = 0;
            int osMinor = 0;
            string[] versionParts = osVersion.Split('.');
            if (versionParts.Length > 0)
                int.TryParse(versionParts[0], out osMajor);
            if (versionParts.Length > 1)
                int.TryParse(versionParts[1], out osMinor);

            return "{"
                + $"osName:'{JsEscape(osName)}',"
                + $"osVersion:'{JsEscape(osVersion)}',"
                + $"osMajor:{osMajor},"
                + $"osMinor:{osMinor},"
                + $"cpuName:'{JsEscape(cpuName)}',"
                + $"cpu64:{(cpu64 ? "true" : "false")},"
                + $"ramGb:{ramGb.ToString("0.0", CultureInfo.InvariantCulture)},"
                + $"diskFreeGb:{diskFreeGb.ToString("0.0", CultureInfo.InvariantCulture)}"
                + "}";
        }

        string JsEscape(string value)
        {
            return (value ?? "")
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }
        
        async Task EseguiRiparazione()
        {
            await webView21.ExecuteScriptAsync("mostraProgress()");

            // 🔹 progress fluido iniziale (parte subito)
            _ = SmoothProgress(0, 40, 40);

            // 🔹 eseguo riparazione (SFC + DISM)
            await repairService.RepairSystem();

            // 🔹 completamento fluido
            await SmoothProgress(40, 100, 15);

            await Task.Delay(100);

            await webView21.ExecuteScriptAsync("nascondiProgress()");
            await webView21.ExecuteScriptAsync("mostraPopup('repair_completed_title', 'repair_completed_desc')");
        }
        async Task EseguiDiagnosi()
        {
            await webView21.ExecuteScriptAsync("mostraProgress()");

            await webView21.ExecuteScriptAsync("aggiornaProgress(20)");

            // 🔹 uso SystemRepairService
            var repairService = new SystemRepairService();

            // 🔹 eseguo diagnosi
            await repairService.DiagnoseSystem();

            await webView21.ExecuteScriptAsync("aggiornaProgress(100)");
            await Task.Delay(100);

            await webView21.ExecuteScriptAsync("nascondiProgress()");

            await webView21.ExecuteScriptAsync(
                "mostraPopup('repair_diagnosis_completed_title', 'repair_diagnosis_completed_desc')"
            );
        }
        async Task EseguiPulizia()
        {
            await webView21.ExecuteScriptAsync("mostraProgress()");

            var repairService = new SystemRepairService();

            // 🔹 avvio pulizia (utente interagisce prima)
            await repairService.CleanSystem();

            // 🔹 SOLO DOPO parte il progress (coerente)
            await SmoothProgress(0, 100, 20);

            await Task.Delay(100);

            await webView21.ExecuteScriptAsync("nascondiProgress()");
            await webView21.ExecuteScriptAsync("mostraPopup('repair_clean_completed_title', 'repair_clean_completed_desc')");
        }
        private async Task ShowLangMessage(string key)
        {
            try
            {
                await webView21.ExecuteScriptAsync($"mostraPopupLang('{key}')");
            }
            catch
            {
                MessageBox.Show(key);
            }
        }
        async Task SmoothProgress(int start, int end, int delay = 50)
        {
            for (int i = start; i <= end; i++)
            {
                await webView21.ExecuteScriptAsync($"aggiornaProgress({i})");
                await Task.Delay(delay);
            }
        }
    }
}
