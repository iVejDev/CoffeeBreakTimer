# CoffeeBreakerTimer

CoffeeBreakerTimer is a cozy cross-platform productivity app built with .NET MAUI. It combines a Pomodoro-style focus timer with a warm coffee-shop workspace: the coffee drains while you focus, refills while you rest, and helps make time feel visual and calm.

## Highlights

- Focus and break timer with custom durations
- Animated coffee mug that syncs with the timer
- Smooth focus/break transitions
- Task list with completion, estimates, and editing
- Link a focus session to the current task
- Statistics dashboard with today's focus time, streaks, and recent sessions
- Rain and chill ambience controls
- Soft session-end sounds
- Windows notifications with in-app fallback
- Timer presets: Classic, Deep work, and Long flow
- Local data management controls
- Cozy dark coffee-shop visual style

## Tech Stack

- .NET MAUI
- CommunityToolkit.MVVM
- MVVM architecture
- Dependency injection
- Clean separation between Core, Infrastructure, and App/UI concerns
- Local persistence with JSON files and MAUI preferences

## Project Structure

```text
CoffeeBreakTimer.Core
  Domain models, enums, interfaces, and timer service logic

CoffeeBreakTimer.Infrastructure
  Reserved for future infrastructure concerns

CoffeeBreakTimer.App
  MAUI app, views, view models, controls, services, assets, and styles
```

## Requirements

- .NET 9 SDK
- .NET MAUI workload
- Windows 10/11 for the current desktop build target

Check installed workloads:

```powershell
dotnet workload list
```

Install MAUI workload if needed:

```powershell
dotnet workload install maui
```

## Build

Debug build for Windows:

```powershell
dotnet build CoffeeBreakTimer.App\CoffeeBreakTimer.App.csproj -f net9.0-windows10.0.19041.0
```

Release build for Windows:

```powershell
dotnet build CoffeeBreakTimer.App\CoffeeBreakTimer.App.csproj -c Release -f net9.0-windows10.0.19041.0
```

## Run

After a debug build, run:

```powershell
CoffeeBreakTimer.App\bin\Debug\net9.0-windows10.0.19041.0\win10-x64\CoffeeBreakTimer.App.exe
```

## Windows Release Exe

To create a shareable Windows release folder:

```powershell
.\scripts\publish-windows-release.ps1
```

The generated executable is placed in:

```text
CoffeeBreakTimer.App\bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\CoffeeBreakTimer.App.exe
```

Share the entire `publish` folder as a zip file. The receiver should extract the zip and double-click `CoffeeBreakTimer.App.exe`. Do not send only the `.exe`, because the app needs the generated dependency files in the same folder.

More details are available in [docs/windows-release.md](docs/windows-release.md).

## Windows MSIX Installer

To create a Windows installer package:

```powershell
.\scripts\publish-windows-msix.ps1
```

The generated package is placed under:

```text
CoffeeBreakTimer.App\bin\Release\net9.0-windows10.0.19041.0\win10-x64\AppPackages
```

MSIX packages must be signed. The script creates or reuses a local self-signed test certificate and exports it to:

```text
build\certificates\CoffeeBreakerTimer_TestCertificate.cer
```

More details are available in [docs/windows-msix-installer.md](docs/windows-msix-installer.md).

## Local Data

The app stores tasks and focus statistics locally on the device. Settings such as ambience, volume, and notification preferences are stored using MAUI preferences.

The Settings page includes cleanup controls for:

- Completed tasks
- Focus statistics
- App preferences

## Notifications

Windows notifications are implemented through the app notification service. If Windows blocks notifications or the notification API is unavailable, CoffeeBreakerTimer falls back to an in-app alert so session completion is still visible.

## Roadmap

- App packaging and installer
- Platform-specific notifications for Android, iOS, and macOS
- Optional import/export for local data
- Theme customization
- More session presets
- Deeper statistics and trends

## Status

CoffeeBreakerTimer is currently a polished first usable version focused on local productivity workflows.
