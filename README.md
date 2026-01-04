[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

<img width="977" height="128" alt="grafik" src="https://github.com/user-attachments/assets/6400ad73-717c-4d68-a852-d7787e70086f" />

<div align="center">
 
  <p>
    <strong>Track and analyze your Path of Exile activities in real time and historically.</strong>
  </p>
  <p>
    <a href="https://www.youtube.com/watch?v=jZ8eiCNOiM4">▶️ Introduction Video</a> |
    <a href="https://github.com/dermow/TraXile/releases/latest">⬇️ Download Latest Release</a>
  </p>
</div>


---

## 📑 Table of Contents

- [ℹ️ About](#ℹ%EF%B8%8F-about)
- [✨ Features](#-features)
- [🚀 Getting Started](#-getting-started)
  - [💾 Installation](#-installation)
  - [🛠️ First Steps](#%EF%B8%8F-first-steps)
  - [⬆️ Update](#%EF%B8%8F-update)
- [❓ FAQ](#-faq)

---

## ℹ️ About

**TraXile** is a tracking and statistics tool for Path of Exile. It reads your game’s client log file and provides real-time and historical statistics about your in-game activities.

TraXile is a potential alternative to "Mapwatch," a similar tool that was recently discontinued. Special thanks to Mapwatch’s developer for the inspiration!

---

## ✨ Features

- **Activity Tracking (Live & Historical) ⏱️**  
  Track time and stats for your in-game activities (maps, heist contracts, sanctum, etc.) both live (while TraXile is open) and retroactively from your Client.txt log file.  
  <img src="https://github.com/dermow/TraXile/blob/master/Docu/Images/NewUI_1.png?raw=true" width="600"/>

- **Comprehensive Statistics 📊**  
  Collect and analyze various game-related statistics.  
  <img src="https://github.com/dermow/TraXile/blob/master/Docu/Images/MappingChart.png?raw=true" width="600"/>

- **Activity Analysis 🔍**  
  Deep-dive into farming strategies and optimize your gameplay with detailed analysis.  
  <img src="https://github.com/dermow/TraXile/blob/master/Docu/Images/SummaryWindow.png?raw=true" width="600"/>

- **In-Game Overlay 🖥️**  
  Keep track of your map time and see which content you’ve encountered, all while playing.  
  <img src="https://github.com/dermow/TraXile/blob/master/Docu/Images/Overlays.png?raw=true" width="600"/>

---

## 🚀 Getting Started

### 💾 Installation

1. **Download** the latest release from the [releases page](https://github.com/dermow/TraXile/releases/latest).
2. **Run** the MSI installer and follow the on-screen instructions.

### 🛠️ First Steps

1. **Select your Client.txt log file:**  
   On first launch, TraXile will try to auto-detect the log file. If not found, select it manually.

   **Default locations:**

   | Platform        | Path                                                                 | Example                                                        |
   |-----------------|----------------------------------------------------------------------|----------------------------------------------------------------|
   | Windows (Steam) | `%SteamLibrary%\steamapps\common\Path of Exile\Logs\Client.txt`      | `C:\Steam\steamapps\common\Path of Exile\logs\Client.txt`      |
   | Windows (Standalone) | `%InstallDirectory%\Logs\Client.txt`                            | `C:\Program Files (x86)\Grinding Gear Games\Path of Exile\logs\Client.txt` |
   | Mac (with Crossover) | `/Users/YOUR_USERNAME/Library/Caches/com.GGG.PathOfExile/Logs/Client.txt` | `/Users/mow/Library/Caches/com.GGG.PathOfExile/Logs/Client.txt` |

2. **Import your data:**  
   TraXile will import all historical data from your log file into its internal database. This may take a moment on first use.

### ⬆️ Update

- TraXile will notify you when a new version is available.  
  If auto-update fails, manually [download the latest release](https://github.com/dermow/TraXile/releases/latest) and install it.

---

## ❓ FAQ

**TraXile isn’t tracking any activities, even with the correct Client.txt path.**  
Make sure your local area chat is enabled; TraXile depends on these log lines.

**Why can't I filter data by character name?**  
Unfortunately, the Client.txt log file does not include the character name.

**Do I need TraXile running while playing to track activities?**  
No. TraXile reads all data in your Client.txt file, so past activities are tracked even if TraXile wasn’t running.

**Why does Windows warn me during installation?**  
As an independent developer, I do not use a Code Signing certificate, which causes Windows to show a warning.

**Does the app send data to the Internet?**  
Only to check for new versions and download some metadata (e.g., league dates).

**Why isn’t this a web-based application?**  
Continuous log file reading is much easier with a desktop app.

**Can I install TraXile on Mac?**  
TraXile is designed for Windows, but you can use Crossover to install it on Mac.  
[Unifer1’s Reddit Guide](https://www.reddit.com/user/Unifer1/):

- Install Crossover
- "Install a new bottle" → "Install an unlisted application"
- Create a Windows 10 64-bit bottle named TraXile
- Select and install the `Setup.msi`
- On first run, select your Client.txt log file
- Should work automatically after setup

**Can I install TraXile on Linux?**  
TraXile is not natively supported on Linux, but it can run successfully using Wine. Here's how:

- Install the required dependencies:
  
  `sudo pacman -S wine mono lib32-gnutls`
  
  `cert-sync /etc/ssl/certs/ca-certificates.crt`

- Run `TraXile.exe` or the MSI installer using Wine: `wine TraXile.exe`

- On first launch, TraXile prompts for your `Client.txt` log file location. Wine uses its own file browser, which cannot see hidden directories such as `.steam`.

  To work around this, create a symlink in your home directory:
  
  `ln -s .steam steam`

  This allows the Wine file browser to navigate into your Steam game folders.

  Once these steps are complete, TraXile should function normally on Linux. Keep in mind that system integration features (like auto-updates) may be limited compared to native Windows.
---

<!-- MARKDOWN LINKS & IMAGES -->
[contributors-shield]: https://img.shields.io/github/contributors/dermow/TraXIle.svg?style=for-the-badge
[contributors-url]: https://github.com/dermow/TraXile/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/dermow/TraXile.svg?style=for-the-badge
[forks-url]: https://github.com/dermow/TraXile/network/members
[stars-shield]: https://img.shields.io/github/stars/dermow/TraXile.svg?style=for-the-badge
[stars-url]: https://github.com/dermow/TraXile/stargazers
[issues-shield]: https://img.shields.io/github/issues/dermow/TraXile.svg?style=for-the-badge
[issues-url]: https://github.com/dermow/TraXile/issues
[license-shield]: https://img.shields.io/github/license/dermow/TraXile.svg?style=for-the-badge
[license-url]: https://github.com/dermow/TraXile/blob/master/LICENSE.txt
