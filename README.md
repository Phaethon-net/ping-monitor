# Network Ping Monitor

A lightweight Windows application that monitors network connectivity by pinging a specified server every 10 seconds and displays the results graphically.

## Features

- Real-time ping monitoring with adjustable intervals (1-100 seconds)
- Visual bar chart display showing online/offline status
- Tracks downtime duration and recovery times
- **Audible alarms** for status changes with mute control
- **Sound notifications**: Rising tone when coming online, warning tone when going offline
- No installation required - single executable file
- Shows 0% or 100% packet loss status
- Persistent settings (remembers IP, interval, and mute preference)

## Requirements

- Windows 10/11 x64
- .NET 8 SDK (for building). End users do NOT need .NET when using the self‑contained single file.

## Usage

1. Run the executable file
2. Enter the IP address or hostname to monitor
3. Click "Start Monitoring" to begin
4. View real-time status in the graphical display

## Building

To build the project for development:

```bash
dotnet build --configuration Release
```

This will build against the `net8.0-windows` target framework.

## Creating Distribution Executable

To create a single, self-contained executable for distribution (matches current project settings – ReadyToRun disabled for size, English resources only):

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Result: `bin\Release\net8.0-windows\win-x64\publish\PingMonitor.exe`

### Current Build Size (reference)

| Variant | Approx Size |
|---------|-------------|
| With ReadyToRun + all cultures (previous) | ~72 MB (single file) |
| No ReadyToRun + English only (current) | ~63-66 MB |

Actual numbers depend on .NET patch version.

### Size / Startup Trade‑offs

- ReadyToRun: Faster cold start (+5–10 MB). Re-enable by setting `<PublishReadyToRun>true</PublishReadyToRun>`.
- Cultures: Additional satellite resource DLLs add several MB. Current csproj sets `<SatelliteResourceLanguages>en</SatelliteResourceLanguages>`.
- Trimming: Disabled (WinForms reflection risk). If experimenting: `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=partial` (highly recommend full regression testing).
- Symbols: Delete `PingMonitor.pdb` before distributing to save a little space.

### Optional Further Reductions

1. Remove `<IncludeNativeLibrariesForSelfExtract>` (slightly slower first launch, sometimes a bit smaller in newer runtimes).
2. Consider upx (external packer) only if corporate policy allows; not required.
3. Evaluate trimming only after creating automated UI/regression tests.

### Reproducible Minimal Command Set

```bash
REM Development build
dotnet build -c Release

REM Distribution build
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Distribution Features

- **Single file**: No installation required, just run the .exe
- **Self-contained**: Includes needed .NET runtime components
- **Optimized**: Compression enabled; ReadyToRun disabled to reduce size
- **Culture-limited**: English resources only to shrink footprint
- **No external dependencies**: End users do not need .NET installed
- **Portable**: Copy the .exe file anywhere and run it
