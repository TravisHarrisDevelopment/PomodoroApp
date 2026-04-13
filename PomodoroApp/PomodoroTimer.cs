namespace PomodoroApp;

public enum PomodoroPhase
{
    Work,
    ShortBreak,
    LongBreak
}

public record SessionEntry(
    string Label,
    PomodoroPhase Phase,
    TimeSpan Duration,
    DateTime StartedAt,
    DateTime CompletedAt
);

public class PomodoroTimer
{
    private System.Windows.Forms.Timer _ticker;
    private TimeSpan _remaining;
    private TimeSpan _phaseDuration;
    private int _completedWorkSessions;

    // Settings
    public TimeSpan WorkDuration { get; set; } = TimeSpan.FromMinutes(25);
    public TimeSpan ShortBreakDuration { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan LongBreakDuration { get; set; } = TimeSpan.FromMinutes(15);
    public int SessionsBeforeLongBreak { get; set; } = 4;

    public PomodoroPhase CurrentPhase { get; private set; } = PomodoroPhase.Work;
    public string CurrentLabel { get; private set; } = string.Empty;
    public bool IsRunning => _ticker.Enabled;
    public TimeSpan Remaining => _remaining;
    public TimeSpan PhaseDuration => _phaseDuration;
    public int CompletedWorkSessions => _completedWorkSessions;
    public DateTime PhaseStartedAt { get; private set; }

    public event Action? Tick;
    public event Action<PomodoroPhase>? PhaseCompleted;
    public event Action<SessionEntry>? SessionLogged;

    public PomodoroTimer()
    {
        _ticker = new System.Windows.Forms.Timer { Interval = 1000 };
        _ticker.Tick += OnTick;
        Reset();
    }

    public void SetLabel(string label) => CurrentLabel = label;

    public void Start()
    {
        if (!IsRunning)
        {
            PhaseStartedAt = DateTime.Now;
            _ticker.Start();
        }
    }

    public void Pause() => _ticker.Stop();

    public void Reset()
    {
        _ticker.Stop();
        CurrentPhase = PomodoroPhase.Work;
        _phaseDuration = WorkDuration;
        _remaining = WorkDuration;
        _completedWorkSessions = 0;
        Tick?.Invoke();
    }

    public void SkipPhase() => CompletePhase();

    private void OnTick(object? sender, EventArgs e)
    {
        _remaining -= TimeSpan.FromSeconds(1);
        Tick?.Invoke();

        if (_remaining <= TimeSpan.Zero)
            CompletePhase();
    }

    private void CompletePhase()
    {
        _ticker.Stop();

        var entry = new SessionEntry(
            Label: CurrentLabel,
            Phase: CurrentPhase,
            Duration: _phaseDuration,
            StartedAt: PhaseStartedAt,
            CompletedAt: DateTime.Now
        );

        if (CurrentPhase == PomodoroPhase.Work)
            _completedWorkSessions++;

        SessionLogged?.Invoke(entry);
        PhaseCompleted?.Invoke(CurrentPhase);

        AdvancePhase();
    }

    private void AdvancePhase()
    {
        CurrentPhase = CurrentPhase switch
        {
            PomodoroPhase.Work when _completedWorkSessions % SessionsBeforeLongBreak == 0
                => PomodoroPhase.LongBreak,
            PomodoroPhase.Work => PomodoroPhase.ShortBreak,
            _ => PomodoroPhase.Work
        };

        _phaseDuration = CurrentPhase switch
        {
            PomodoroPhase.Work => WorkDuration,
            PomodoroPhase.ShortBreak => ShortBreakDuration,
            PomodoroPhase.LongBreak => LongBreakDuration,
            _ => WorkDuration
        };

        _remaining = _phaseDuration;
        Tick?.Invoke();
    }
}
