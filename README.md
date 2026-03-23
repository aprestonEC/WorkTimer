# WorkTimer

A lightweight Windows system tray app that tracks elapsed time and keeps your computer awake.

Built for developers and anyone who runs long-running processes (AI agents, builds, deployments) and needs their machine to stay awake without fiddling with power settings.

## Features

- **Elapsed timer** — hover the tray icon to see how long you've been working
- **Keep-awake** — prevents Windows from sleeping using `SetThreadExecutionState` plus a periodic simulated key press (F13–F24, configurable)
- **Pause / Resume** — pause the timer and release keep-awake when you step away
- **Configurable** — choose which key is simulated and how often (Options menu)
- **Single instance** — won't launch duplicates if you accidentally double-click

## Install

Download `WorkTimer.msi` from the [latest release](https://github.com/aprestonEC/WorkTimer/releases/latest) and run it. The installer is self-contained — no .NET runtime required.

Installs to `%LocalAppData%\WorkTimer` with a Start Menu shortcut.

## Usage

Launch **WorkTimer** from the Start Menu. A clock icon appears in the system tray.

| Action | How |
|---|---|
| See elapsed time | Hover the tray icon |
| Pause / Resume | Right-click > Pause |
| Reset timer | Right-click > Reset Timer |
| Change key / interval | Right-click > Options... |
| Quit | Right-click > Exit |

Settings are saved to `%LocalAppData%\WorkTimer\settings.json`.

## Build from source

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```
git clone https://github.com/aprestonEC/WorkTimer.git
cd WorkTimer
dotnet run
```

To build the MSI installer:

```
dotnet publish -c Release
dotnet tool install --global wix
wix build installer/WorkTimer.wxs -d "PublishDir=bin/Release/net10.0-windows/win-x64/publish" -o bin/Release/WorkTimer.msi
```
