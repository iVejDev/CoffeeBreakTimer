# Windows MSIX installer

CoffeeBreakerTimer can also be published as a Windows MSIX installer package.

MSIX gives a cleaner install experience than the unpackaged `.exe` folder, but Windows requires MSIX packages to be signed.

For this project, the release script creates or reuses a local self-signed test certificate:

```text
CN=CoffeeBreakerTimer
```

This is suitable for school demos and testing with friends. For public distribution, use Microsoft Store signing or a trusted code-signing certificate.

## Build the MSIX package

From the repository root:

```powershell
.\scripts\publish-windows-msix.ps1
```

## Output location

The generated package is created under:

```text
CoffeeBreakTimer.App\bin\Release\net9.0-windows10.0.19041.0\win10-x64\AppPackages
```

The script also exports the test certificate here:

```text
build\certificates\CoffeeBreakerTimer_TestCertificate.cer
```

## Install on your own computer

After publishing, open the generated `AppPackages` folder and install the generated `.msix` package.

If Windows says the publisher is not trusted, install the exported certificate first:

1. Double-click `CoffeeBreakerTimer_TestCertificate.cer`.
2. Choose `Install Certificate`.
3. Select `Local Machine` if available, otherwise `Current User`.
4. Place it in `Trusted People`.
5. Install the `.msix` package again.

## Share with friends

Send both:

- the generated `.msix` package
- `CoffeeBreakerTimer_TestCertificate.cer`

Your friend must install/trust the certificate before installing the MSIX package. This is normal for self-signed MSIX packages.

For a smoother public install experience, publish through Microsoft Store or sign with a trusted code-signing certificate.
