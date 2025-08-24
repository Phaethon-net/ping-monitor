# Network Ping Monitor

A lightweight Windows application that monitors network connectivity by pinging a specified server every 10 seconds and displays the results graphically.

## Features

- Real-time ping monitoring with 10-second intervals
- Visual bar chart display showing online/offline status
- Tracks downtime duration and recovery times
- No installation required - single executable file
- Shows 0% or 100% packet loss status

## Requirements

- Windows OS
- .NET Framework 4.8 or later

## Usage

1. Run the executable file
2. Enter the IP address or hostname to monitor
3. Click "Start Monitoring" to begin
4. View real-time status in the graphical display

## Building

To build the project:

```bash
dotnet build --configuration Release --self-contained true --runtime win-x64
```

To create a single executable file:

```bash
dotnet publish --configuration Release --self-contained true --runtime win-x64 /p:PublishSingleFile=true
```
