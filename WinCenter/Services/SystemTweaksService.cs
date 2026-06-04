using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WinCenter.Services
{
    public class SystemTweaksService
    {
        const string ExplorerAdvanced = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        const string Personalize = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        const string Search = @"Software\Microsoft\Windows\CurrentVersion\Search";

        public string GetStatusJson()
        {
            bool hibernation = File.Exists(Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "hiberfil.sys"));

            return "{"
                + Status("showExtensions", ReadInt(ExplorerAdvanced, "HideFileExt", 1) == 0)
                + Status("showHidden", ReadInt(ExplorerAdvanced, "Hidden", 2) == 1)
                + Status("darkTheme", ReadInt(Personalize, "AppsUseLightTheme", 1) == 0)
                + Status("disableHibernation", !hibernation)
                + Status("bingSearch", ReadInt(Search, "BingSearchEnabled", 1) != 0)
                + Status("centerTaskbar", ReadInt(ExplorerAdvanced, "TaskbarAl", 0) == 1)
                + Status("mouseAcceleration", ReadString(@"Control Panel\Mouse", "MouseSpeed", "1") != "0")
                + Status("numLock", ReadString(@"Control Panel\Keyboard", "InitialKeyboardIndicators", "0").EndsWith("2"))
                + Status("recommendations", ReadInt(ExplorerAdvanced, "Start_IrisRecommendations", 1) != 0)
                + Status("searchButton", ReadInt(Search, "SearchboxTaskbarMode", 1) != 0)
                + Status("taskView", ReadInt(ExplorerAdvanced, "ShowTaskViewButton", 1) != 0)
                + Status("disableFolderDiscovery", ReadString(@"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell", "FolderType", "") == "NotSpecified")
                + Status("disableStoreSearch", ReadInt(@"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 0) == 1)
                + Status("disablePwshTelemetry", ReadString(@"Environment", "POWERSHELL_TELEMETRY_OPTOUT", "") == "1")
                + Status("enableEndTask", ReadInt(ExplorerAdvanced + @"\TaskbarDeveloperSettings", "TaskbarEndTask", 0) == 1)
                + Status("disableActivityHistory", ReadMachineInt(@"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 1) == 0)
                + Status("disableConsumerFeatures", ReadMachineInt(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 0) == 1)
                + Status("disableLocation", ReadMachineInt(@"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", 0) == 1)
                + Status("disableTelemetry", ReadMachineInt(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 1) == 0)
                + Status("removeWidgets", ReadMachineInt(@"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 1) == 0)
                + Status("disableCopilot", ReadInt(@"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 0) == 1)
                + Status("disableBackgroundApps", ReadInt(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 0) == 1)
                + Status("disableFullscreen", ReadInt(@"System\GameConfigStore", "GameDVR_FSEBehaviorMode", 0) == 2)
                + Status("disableStorageSense", ReadInt(@"Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01", 1) == 0)
                + Status("removeGallery", ReadInt(@"Software\Classes\CLSID\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", "System.IsPinnedToNameSpaceTree", 1) == 0)
                + Status("classicContextMenu", Registry.CurrentUser.OpenSubKey(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32") != null)
                + Status("detailedBsod", ReadMachineInt(@"SYSTEM\CurrentControlSet\Control\CrashControl", "DisplayParameters", 0) == 1)
                + Status("disableMpo", ReadMachineInt(@"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", 0) == 5)
                + Status("s3Sleep", ReadMachineInt(@"SYSTEM\CurrentControlSet\Control\Power", "PlatformAoAcOverride", 1) == 0)
                + Status("verboseLogon", ReadMachineInt(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "VerboseStatus", 0) == 1)
                + Status("disableIPv6", ReadMachineInt(@"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters", "DisabledComponents", 0) == 255)
                + Status("preferIPv4", ReadMachineInt(@"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters", "DisabledComponents", 0) == 32)
                + Status("utcTime", ReadMachineInt(@"SYSTEM\CurrentControlSet\Control\TimeZoneInformation", "RealTimeIsUniversal", 0) == 1)
                + LastStatus("stickyKeys", ReadString(@"Control Panel\Accessibility\StickyKeys", "Flags", "510") == "510")
                + "}";
        }

        public async Task<bool> SetToggle(string key, bool enabled)
        {
            try
            {
                if (key == "disableHibernation") return await SetHibernation(!enabled);
                if (key == "disableActivityHistory") return await SetMachineDword(@"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", enabled ? 0 : 1);
                if (key == "disableConsumerFeatures") return await SetMachineDword(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", enabled ? 1 : 0);
                if (key == "disableLocation") return await SetMachineDword(@"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", enabled ? 1 : 0);
                if (key == "disableTelemetry") return await SetMachineDword(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", enabled ? 0 : 1);
                if (key == "removeWidgets") return await SetMachineDword(@"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", enabled ? 0 : 1);
                if (key == "detailedBsod") return await SetMachineDword(@"SYSTEM\CurrentControlSet\Control\CrashControl", "DisplayParameters", enabled ? 1 : 0);
                if (key == "disableMpo") return await SetMachineDword(@"SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", enabled ? 5 : 0);
                if (key == "s3Sleep") return await SetMachineDword(@"SYSTEM\CurrentControlSet\Control\Power", "PlatformAoAcOverride", enabled ? 0 : 1);
                if (key == "verboseLogon") return await SetMachineDword(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "VerboseStatus", enabled ? 1 : 0);
                if (key == "disableIPv6") return await SetMachineDword(@"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters", "DisabledComponents", enabled ? 255 : 0);
                if (key == "preferIPv4") return await SetMachineDword(@"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters", "DisabledComponents", enabled ? 32 : 0);
                if (key == "disableTeredo") return await RunElevated("netsh.exe", "interface teredo set state " + (enabled ? "disabled" : "default"));
                if (key == "utcTime") return await SetMachineDword(@"SYSTEM\CurrentControlSet\Control\TimeZoneInformation", "RealTimeIsUniversal", enabled ? 1 : 0);

                if (key == "showExtensions") WriteInt(ExplorerAdvanced, "HideFileExt", enabled ? 0 : 1);
                else if (key == "showHidden") WriteInt(ExplorerAdvanced, "Hidden", enabled ? 1 : 2);
                else if (key == "darkTheme") { WriteInt(Personalize, "AppsUseLightTheme", enabled ? 0 : 1); WriteInt(Personalize, "SystemUsesLightTheme", enabled ? 0 : 1); }
                else if (key == "bingSearch") WriteInt(Search, "BingSearchEnabled", enabled ? 1 : 0);
                else if (key == "centerTaskbar") WriteInt(ExplorerAdvanced, "TaskbarAl", enabled ? 1 : 0);
                else if (key == "mouseAcceleration") { WriteString(@"Control Panel\Mouse", "MouseSpeed", enabled ? "1" : "0"); WriteString(@"Control Panel\Mouse", "MouseThreshold1", enabled ? "6" : "0"); WriteString(@"Control Panel\Mouse", "MouseThreshold2", enabled ? "10" : "0"); }
                else if (key == "numLock") WriteString(@"Control Panel\Keyboard", "InitialKeyboardIndicators", enabled ? "2" : "0");
                else if (key == "recommendations") WriteInt(ExplorerAdvanced, "Start_IrisRecommendations", enabled ? 1 : 0);
                else if (key == "searchButton") WriteInt(Search, "SearchboxTaskbarMode", enabled ? 1 : 0);
                else if (key == "taskView") WriteInt(ExplorerAdvanced, "ShowTaskViewButton", enabled ? 1 : 0);
                else if (key == "stickyKeys") WriteString(@"Control Panel\Accessibility\StickyKeys", "Flags", enabled ? "510" : "506");
                else if (key == "disableFolderDiscovery") SetOrDeleteString(@"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell", "FolderType", enabled, "NotSpecified");
                else if (key == "disableStoreSearch") WriteInt(@"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", enabled ? 1 : 0);
                else if (key == "disablePwshTelemetry") SetOrDeleteString(@"Environment", "POWERSHELL_TELEMETRY_OPTOUT", enabled, "1");
                else if (key == "enableEndTask") WriteInt(ExplorerAdvanced + @"\TaskbarDeveloperSettings", "TaskbarEndTask", enabled ? 1 : 0);
                else if (key == "disableCopilot") WriteInt(@"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", enabled ? 1 : 0);
                else if (key == "disableBackgroundApps") WriteInt(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", enabled ? 1 : 0);
                else if (key == "disableFullscreen") WriteInt(@"System\GameConfigStore", "GameDVR_FSEBehaviorMode", enabled ? 2 : 0);
                else if (key == "disableStorageSense") WriteInt(@"Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01", enabled ? 0 : 1);
                else if (key == "removeGallery") WriteInt(@"Software\Classes\CLSID\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", "System.IsPinnedToNameSpaceTree", enabled ? 0 : 1);
                else if (key == "classicContextMenu") SetClassicContextMenu(enabled);
                else return false;

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Errore modifica impostazione " + key, ex);
                return false;
            }
        }

        public Task<bool> RunAction(string action)
        {
            if (action == "restore_point") return CreateRestorePoint();
            if (action == "delete_temp") return DeleteTemporaryFiles();
            if (action == "ultimate_performance") return EnableUltimatePerformance();
            if (action == "remove_ultimate") return RemoveUltimatePerformance();
            if (action == "control_panel") return OpenWindow("control.exe");
            if (action == "performance_dialog") return OpenWindow("SystemPropertiesPerformance.exe");
            if (action == "oosu_website") return OpenWindow("https://www.oo-software.com/en/shutup10");
            if (action.StartsWith("dns_")) return SetDns(action.Substring(4));
            return Task.FromResult(false);
        }

        public Task<bool> SetHibernation(bool enabled)
        {
            return RunElevated("powercfg.exe", "/hibernate " + (enabled ? "on" : "off"));
        }

        public Task<bool> CreateRestorePoint()
        {
            const string command = "Checkpoint-Computer -Description 'WinCenter Restore Point' -RestorePointType 'MODIFY_SETTINGS'";
            return RunElevated("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + command + "\"");
        }

        public Task<bool> DeleteTemporaryFiles()
        {
            const string command = "Get-ChildItem -LiteralPath $env:TEMP -Force -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue";
            return RunElevated("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + command + "\"");
        }

        public Task<bool> EnableUltimatePerformance()
        {
            const string guid = "e9a42b02-d5df-448d-aa00-03f14749eb61";
            return RunElevated("cmd.exe", "/c powercfg -duplicatescheme " + guid + " " + guid + " >nul 2>&1 & powercfg /setactive " + guid);
        }

        public Task<bool> RemoveUltimatePerformance()
        {
            const string guid = "e9a42b02-d5df-448d-aa00-03f14749eb61";
            return RunElevated("cmd.exe", "/c powercfg /setactive SCHEME_BALANCED & powercfg /delete " + guid);
        }

        public Task<bool> SetDns(string provider)
        {
            string servers = provider == "cloudflare" ? "'1.1.1.1','1.0.0.1'" : provider == "google" ? "'8.8.8.8','8.8.4.4'" : "";
            string command = provider == "default"
                ? "Get-DnsClientServerAddress | Where-Object {$_.InterfaceAlias -notmatch 'Loopback'} | Set-DnsClientServerAddress -ResetServerAddresses"
                : "Get-DnsClientServerAddress | Where-Object {$_.InterfaceAlias -notmatch 'Loopback'} | Set-DnsClientServerAddress -ServerAddresses " + servers;
            return RunElevated("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -Command \"" + command + "\"");
        }

        Task<bool> SetMachineDword(string path, string name, int value)
        {
            string args = "ADD \"HKLM\\" + path + "\" /v \"" + name + "\" /t REG_DWORD /d " + value + " /f";
            return RunElevated("reg.exe", args);
        }

        static int ReadInt(string path, string name, int fallback)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(path))
            {
                if (key == null) return fallback;
                return Convert.ToInt32(key.GetValue(name, fallback));
            }
        }

        static string ReadString(string path, string name, string fallback)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(path))
            {
                return key == null ? fallback : Convert.ToString(key.GetValue(name, fallback));
            }
        }

        static int ReadMachineInt(string path, string name, int fallback)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path))
            {
                if (key == null) return fallback;
                return Convert.ToInt32(key.GetValue(name, fallback));
            }
        }

        static void WriteInt(string path, string name, int value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(path))
                key.SetValue(name, value, RegistryValueKind.DWord);
        }

        static void WriteString(string path, string name, string value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(path))
                key.SetValue(name, value, RegistryValueKind.String);
        }

        static void SetOrDeleteString(string path, string name, bool enabled, string value)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(path))
            {
                if (enabled) key.SetValue(name, value, RegistryValueKind.String);
                else key.DeleteValue(name, false);
            }
        }

        static void SetClassicContextMenu(bool enabled)
        {
            const string path = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";
            if (enabled)
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(path + @"\InprocServer32"))
                    key.SetValue("", "", RegistryValueKind.String);
            }
            else
            {
                Registry.CurrentUser.DeleteSubKeyTree(path, false);
            }
        }

        static string Status(string key, bool value) { return key + ":" + (value ? "true" : "false") + ","; }
        static string LastStatus(string key, bool value) { return key + ":" + (value ? "true" : "false"); }

        static Task<bool> OpenWindow(string target)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true });
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Logger.LogError("Errore apertura strumento: " + target, ex);
                return Task.FromResult(false);
            }
        }

        static async Task<bool> RunElevated(string executable, string arguments)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Process process = Process.Start(new ProcessStartInfo
                    {
                        FileName = executable,
                        Arguments = arguments,
                        Verb = "runas",
                        UseShellExecute = true
                    });

                    if (process == null) return false;
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
                catch (Exception ex)
                {
                    Logger.LogError("Errore comando tweak: " + executable, ex);
                    return false;
                }
            });
        }
    }
}
