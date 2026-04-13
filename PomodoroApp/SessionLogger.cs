namespace PomodoroApp;

/// <summary>
/// Handles logging of completed Pomodoro sessions to the file system.
/// Logs are written to %APPDATA%\PomodoroApp\sessions.log
/// </summary>
public class SessionLogger
{
    private readonly string _logPath;
    private readonly bool _enabled;

    public SessionLogger(bool enabled = true)
    {
        _enabled = enabled;
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "PomodoroApp");
        Directory.CreateDirectory(dir);
        _logPath = Path.Combine(dir, "sessions.log");
    }

    public void Log(SessionEntry entry)
    {
        if (!_enabled) return;

        try
        {
            var line = FormatEntry(entry);
            File.AppendAllText(_logPath, line + Environment.NewLine);
        }
        catch (IOException)
        {
            // Swallow IO errors — logging should never crash the app
        }
    }

    public string LogFilePath => _logPath;

    private static string FormatEntry(SessionEntry entry)
    {
        var phase = entry.Phase switch
        {
            PomodoroPhase.Work => "WORK",
            PomodoroPhase.ShortBreak => "SHORT BREAK",
            PomodoroPhase.LongBreak => "LONG BREAK",
            _ => "UNKNOWN"
        };

        var label = string.IsNullOrWhiteSpace(entry.Label) ? "(unlabeled)" : entry.Label;

        return $"{entry.CompletedAt:yyyy-MM-dd HH:mm:ss} | {phase,-12} | {entry.Duration:mm\\:ss} | {label}";
    }
}
