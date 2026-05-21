# Windows release build

This project is configured to publish CoffeeBreakerTimer as an unpackaged Windows app.

The result is a normal `.exe` plus the files it needs to run. Share the entire publish folder, not only the `.exe`.

## Build the release

From the repository root:

```powershell
.\scripts\publish-windows-release.ps1
```

Or run the publish command directly:

```powershell
dotnet publish CoffeeBreakTimer.App\CoffeeBreakTimer.App.csproj -f net9.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64 -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true -p:PublishProfile=Windows-Unpackaged-SelfContained
```

## Output location

After publishing, the app is generated here:

```text
CoffeeBreakTimer.App\bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish
```

The executable is:

```text
CoffeeBreakTimer.App.exe
```

## Share with friends

Zip the whole `publish` folder and send the zip file.

The receiver should extract the zip first, then double-click:

```text
CoffeeBreakTimer.App.exe
```

Do not send only the `.exe`, because the app also needs the generated dependency files in the same folder.

## Notes

- App name: CoffeeBreakerTimer
- Version: 1.0.0
- Windows package type: unpackaged executable
- Windows App SDK deployment: self-contained
- Publish profile: `CoffeeBreakTimer.App\Properties\PublishProfiles\Windows-Unpackaged-SelfContained.pubxml`

MSIX packaging can be added later if the app should have a formal installer, Start Menu integration, and signing.
