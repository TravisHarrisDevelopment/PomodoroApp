# Pomodoro App

A personal Pomodoro timer for Windows 11, built with C# and .NET 10 WinForms.

## Features

- Customizable work, short break, and long break durations
- Session labeling — tag what you're working on
- Alarm sound at the end of each phase (WAV or system sound fallback)
- Minimize to system tray — hover the icon to see time remaining
- Balloon tip notifications when a phase completes
- Dark UI with per-phase color accents
- Session logging to `%APPDATA%\PomodoroApp\sessions.log`

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 11 (or Windows 10)

## Getting Started

```bash
# Clone / copy the project folder, then:
cd PomodoroApp
dotnet run
```

Or open in Visual Studio 2022+ and press F5.

## Adding a Custom Alarm Sound

Place a `.wav` file in the project directory and update `AlarmPlayer` construction in `MainForm.cs`:

```csharp
private readonly AlarmPlayer _alarm = new("alarm.wav");
```

If no WAV is provided, the app falls back to the Windows system exclamation sound.

## Project Structure

```
PomodoroApp/
├── Program.cs          — Entry point
├── MainForm.cs         — Main window + tray icon orchestration
├── PomodoroTimer.cs    — Core timer logic, phase transitions, events
├── AlarmPlayer.cs      — WAV playback with system sound fallback
├── SessionLogger.cs    — File system logging (%APPDATA%\PomodoroApp\)
├── SettingsForm.cs     — Settings dialog
└── app.manifest        — DPI awareness + Windows 11 compatibility
```

## Logging

Sessions are logged automatically to:
```
%APPDATA%\PomodoroApp\sessions.log
```

Format:
```
2025-04-06 09:30:00 | WORK         | 25:00 | Write unit tests
2025-04-06 09:35:00 | SHORT BREAK  | 05:00 | (unlabeled)
```

To disable logging, change `enabled: true` to `enabled: false` in `MainForm.cs`.
