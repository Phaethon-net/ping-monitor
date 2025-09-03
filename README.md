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

- Windows OS
- .NET Framework 4.8 or later

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

## Creating Distribution Executable

To create a single, self-contained executable for distribution:

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

The executable will be created at: `bin\Release\net5.0-windows\win-x64\publish\PingMonitor.exe`

### Distribution Features

- **Single file**: No installation required, just run the .exe
- **Self-contained**: Includes all .NET runtime dependencies
- **Optimized**: Trimmed for smaller file size (~135MB)
- **No dependencies**: Runs on Windows without requiring .NET installation
- **Portable**: Copy the .exe file anywhere and run it
