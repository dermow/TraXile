<!-- Improved compatibility of back to top link: See: https://github.com/othneildrew/Best-README-Template/pull/73 -->
<a name="readme-top"></a>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

<div align="center">
  <a href="https://github.com/dermow/TraXile">
    <img src="https://github.com/dermow/TraXile/blob/master/TraXile/Resources/trax.jpg?raw=true" />
  </a>
</div>

# Table of Contents
<!--ts-->
   * [About](#About)
   * [Features](#Features)
   * [Getting Started](#Getting-Started)
        * [Video](#Video)
        * [Installation](#Installation)
        * [First Steps](#First-Steps)
        * [Update](#Update)
   * [FAQ](#FAQ)
<!--te-->

# About
Traxile is a tracking and statistics tool for Path of Exile. It reads your client log file and provides you with real-time and long-term statistics for your in-game activities. Download Traxile from GitHub, install it, and select your log file. The tool gives you detailed information about your maps, boss fights, and other activities. Additionally, Traxile offers a "Stop Watch Overlay," allowing you to see your map time directly in-game.

Traxile can be a potential alternative to "Mapwatch", a similar tool that was recently discontinued. Many thanks to the developer of Mapwatch, whose work inspired me to create this tool.

You can check out my introduction video to get started:
https://www.youtube.com/watch?v=jZ8eiCNOiM4

# Features
## Activity Tracking (Live and backwards)
Track time and other stats for your ingame activities (maps, heist contracts, sanctum...). Works for live tracking (TraXile open while you are playing)
and backwards for all data in your Client.txt logfile.



## Ingame Overlay
<img src="https://github.com/dermow/TraXile/blob/master/Docu/Images/Traxile_Screenshot_Overlay.png?raw=true" />

## Statistic collection
Collect statistics around the Game.

## Data visualization
Visualize collected statistics in charts and tables.



## Installation
Download the latest release and execute the MSI-Installer file.

## First steps
After the first start, you have to select the path to your Client.txt logfile. The most common default paths listed below:

| Client  | Path | Example |
|---|---|---|
| Windows (Steam)  | %SteamLibrary%\steamapps\commmon\Path of Exile\Logs\Client.txt  | C:\Steam\steamapps\common\Path of Exile\logs\Client.txt |
| Windows (Standalone)  | %InstallDirectory%\Logs\Client.txt  | C:\Program Files (x86)\Grinding Gear Games\Path of Exile\logs\Client.txt |
| Mac OS (with Crossover or similar)  | /Users/YOUR_USERNAME/Library/Caches/com.GGG.PathOfExile/Logs/Client.txt  | /Users/mow/Library/Caches/com.GGG.PathOfExile/Logs/Client.txt |

After you selected the logfile, TraXile will start importing all data thats inside it - and writes that data in its own database. After the initial import, the start of TraXile will
be much faster.

## Update
You will be automatically notified when a new version is available. Just follow the instructions to update.
If this is not working for any reason, you can always download the latest relase here and install it manually.

# FAQ
### Why can´t the data be filtered by character name?
Unfornately, the name of your in character is not written to the Client logfile.

### Are only activities tracked if I have TraXile running while playing?
No, TraXile reads all data that is in your Client.txt logfile, so even If TraXile is not running, your maps and stuff like that will still appear in history. But you have a bunch of
options while TraXile is running - for exampel you can manually pause the stopwatch.

### Why is Windows warning me during the installation?
As I am developing TraXile completely in my sparetime, I am not paying for a Code Signing certificate, which is needed to be a trusted developer for Windows.

### Is this app sending any data to the Internet?
Only internet connections are the check for a new version and the download of some metadata (league start and end-dates for example)

### I ran two maps of the same area - and TraXile counted them as one map?
This could happen if you are running the same map multiple time in a row, and randomly two maps in a row gets the same server instance. In that case TraXile has no chance to 
seperate this maps automatically. There is an ingame chat command "trax::split" you could use to solve this (if it is very important for you).

### Why isn´t that a web based appliation?
Because TraXile permanently reads you Client.txt logfile - thats much easier with a desktop application.

### Can I install TraXile on my Mac?
TraXile is designed for Windows computers. But you can use Crossover to install it on mac anyway. Here is a short How-To (thanks to reddit user [Unifer1](https://www.reddit.com/user/Unifer1/))

    * Install the newest version of Crossover
    * Click "Install a new bottle"
    * Click "Install an unlisted application"
    * Create a new Windows 10 64-bit bottle named Traxile
    * Select the "Setup.msi" installer file
    * Click Install
    * Once the install is done, open Traxile.
    * On first open, you'll need to find the Client.txt logfile; on macOS it's located at: /Users/[yourusername]/Library/Caches/com.GGG.PathOfExile/Logs/Client.txt
    * Should work automatically every time you start up from now on!

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
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
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/linkedin_username
[product-screenshot]: images/screenshot.png
