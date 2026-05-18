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
