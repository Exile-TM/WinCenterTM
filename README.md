<div align="center">
  <img src="https://i.ibb.co/sd81sM0P/Logo-Win-Center.png" alt="WinCenter logo" width="430">

  <p><strong>Everything you need to manage, protect and customise Windows — all in one place.</strong></p>

  <p>
    <a href="https://github.com/Exile-TM/WinCenterTM/releases/latest"><img src="https://img.shields.io/github/v/release/Exile-TM/WinCenterTM?display_name=tag&sort=semver&label=release&color=2ea44f" alt="Latest release"></a>
    <a href="https://github.com/Exile-TM/WinCenterTM/releases"><img src="https://img.shields.io/github/downloads/Exile-TM/WinCenterTM/total?label=downloads&color=0078d4" alt="Total downloads"></a>
    <img src="https://img.shields.io/badge/Windows-7%20%7C%2010%20%7C%2011-0078d4?logo=windows" alt="Windows 7, 10 and 11">
    <img src="https://img.shields.io/badge/.NET%20Framework-4.8-512bd4" alt=".NET Framework 4.8">
    <img src="https://img.shields.io/badge/licence-proprietary-dc3545" alt="Proprietary licence">
  </p>

  <p>
    <a href="https://github.com/Exile-TM/WinCenterTM/releases/latest"><strong>Download the latest version</strong></a>
    ·
    <a href="https://github.com/Exile-TM/WinCenterTM/releases">Release notes</a>
    ·
    <a href="https://github.com/Exile-TM/WinCenterTM/issues">Report an issue</a>
    ·
    <a href="https://www.youtube.com/@RossettoPaolo">YouTube channel</a>
  </p>
</div>

![WinCenter home screen](https://i.ibb.co/TDwNk5r4/wincenter-home.png)

## What is WinCenter™?

**WinCenter™** is a desktop suite that brings Windows management, maintenance, security and customisation tools together in a single interface.

It is designed to reduce the time spent moving between Control Panel pages, system commands, download websites and separate utilities. Everyday features remain easy to reach, while advanced tools are organised into dedicated sections with clear descriptions and compatibility checks.

WinCenter is developed by **Paolo Rossetto / Exile-TM** for the **Informatica Spiegata Male** YouTube channel. It is an independent project and is not affiliated with Microsoft...and luckily, I’d say…

## What can you do with it?

| Area | Main features |
| --- | --- |
| **PC diagnostics and health** | System information and checks covering Windows, storage, memory, startup items, security and updates. |
| **Maintenance and repair** | Cleanup, optimisation, startup management and tools for identifying or repairing Windows problems. |
| **Security and privacy** | Antivirus and firewall status, security scans, Privacy Guard and controls for privacy-sensitive Windows settings. |
| **Drivers and updates** | Update management, driver inventory, driver backups and Driver Store maintenance tools. |
| **MegaISO®** | Guided creation of customised Windows installation media with drivers, updates, languages, components and multiple editions. |
| **Software and utilities** | An organised catalogue of essential software, Microsoft tools, internet applications, multimedia software, development tools, document utilities and file-sharing programs. |
| **Backup and post-installation** | Tools for preparing, saving and restoring useful configurations during maintenance or after reinstalling Windows. |
| **WinCenter™ experience** | Integrated search, themes, customisable tiles, a quick panel, in-app updates and an interface available in more than 20 languages. |
| **WinCenter™ multimedia** | A collection of lightweight games that can be launched instantly from within WinCenter. A built-in player for listening to local music without leaving the application. WinCenter HUB — coming soon — A new central space for discovering and accessing WinCenter services, features and connected experiences. |

> Some operations change system settings or require administrator privileges. WinCenter™ explains the relevant steps and requests elevation only when it is needed.

## WinCenter™ Free and WinCenter™ Pro

The free edition provides access to the core suite, the software catalogue and many included utilities. **WinCenter™ Pro** unlocks additional advanced tools for diagnostics, privacy, repair, performance and PC management.

Pro is activated directly inside WinCenter™ using a personal licence key. Current details about included features and purchase options are always available in the **Pro** section of the application.

## Download and installation

1. Open the [latest official release](https://github.com/Exile-TM/WinCenterTM/releases/latest).
2. Expand the **Assets** section and download `WinCenter_Setup_vX.Y.exe`.
3. Run the installer and follow the setup wizard.
4. Launch WinCenter™ from the Start menu. The application may request administrator privileges when you use features that make system-level changes.

When included in a release, `WinCenter.Remote.apk` can also be downloaded as the optional Android companion app used to connect a phone to WinCenter™. It is not required to use the desktop suite.

> **SmartScreen notice:** the installers currently distributed are not digitally signed. Windows may therefore display an “Unknown publisher” warning. Only download WinCenter from this repository and its official Releases section.

## Requirements

- Windows 7 SP1, Windows 10 or Windows 11.
- Microsoft .NET Framework 4.8.
- Microsoft Edge WebView2 Runtime.
- An internet connection for the online catalogue, downloads and update checks.
- Administrator privileges for features that modify Windows, drivers, the firewall or system images.

WebView2 is normally already available on Windows 10 and Windows 11. If the interface does not load, install or repair the [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/).

## MegaISO®

MegaISO® is WinCenter™'s environment for preparing customised Windows installation images. Its guided workflow lets you choose the source image, Windows editions and resources to integrate, while validating the configuration before the build begins.

Available operations include:

- managing multiple Windows editions in the same ISO;
- integrating drivers, updates and language packs;
- customising components and the final output;
- using profiles designed for different Windows versions;
- detecting incompatible combinations before the build starts;
- following progress and detailed logs throughout the process.

MegaISO® works with Windows images and installation files. Always use original sources, keep a backup of important data and test the resulting media before using it on a production computer.

## Project status

WinCenter™ is under **active development**. Stable versions, release notes and official downloads are published in [GitHub Releases](https://github.com/Exile-TM/WinCenterTM/releases).

The project is primarily built with:

- C# and Windows Forms;
- .NET Framework 4.8;
- Microsoft Edge WebView2 for the interface;
- Inno Setup for installer distribution.

## Issues and support

Found a problem or have a feature suggestion? [Open an issue](https://github.com/Exile-TM/WinCenterTM/issues) and include, whenever possible:

- your WinCenter™ version;
- your Windows version and edition;
- the steps required to reproduce the problem;
- the expected and actual results;
- relevant screenshots or error messages, without personal information or licence keys.

For guides, demonstrations and project updates, visit the [Informatica Spiegata Male](https://www.youtube.com/@RossettoPaolo) YouTube channel.

## Licence

WinCenter™ is **proprietary software — all rights reserved**.

The presence of source code in this repository does not grant permission to copy, modify, redistribute, publish or use it, in whole or in part, without the author's explicit permission.

## Trademarks and disclaimer

WinCenter™ is an independent project and is not affiliated with, endorsed by or sponsored by Microsoft Corporation. Microsoft, Windows and all other names or trademarks mentioned belong to their respective owners.

The software is provided “as is”. Before applying significant system changes or creating installation media, back up your data and review the selected options carefully.
