# CoffeeBreakerTimer

CoffeeBreakerTimer is a cozy .NET MAUI productivity app built around focus sessions, gentle breaks, and a coffee mug that visually drains and refills with the timer.

## Features

- Focus and break timer with synchronized coffee animation
- Cozy dark coffee-shop UI
- Local task list with completion and focus-session estimates
- Link focus sessions to a current task
- Session statistics and recent focus history
- Rain and chill ambience controls
- Timer presets for classic, deep work, and long flow sessions
- Prepared notification service abstraction for future OS notifications

## Tech

- .NET MAUI
- MVVM with CommunityToolkit.MVVM
- Dependency injection
- Clean separation between Core services, app services, and UI
- Local JSON and preferences persistence

## Run

```powershell
dotnet build CoffeeBreakTimer.App\CoffeeBreakTimer.App.csproj -f net9.0-windows10.0.19041.0
```

On Windows, run the built app from:

```powershell
CoffeeBreakTimer.App\bin\Debug\net9.0-windows10.0.19041.0\win10-x64\CoffeeBreakTimer.App.exe
```
