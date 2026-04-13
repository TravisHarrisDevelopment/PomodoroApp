namespace PomodoroApp;

public class MainForm : Form
{
    private readonly PomodoroTimer _timer = new();
    private readonly AlarmPlayer _alarm = new();
    private readonly SessionLogger _logger = new(enabled: true);
	private readonly Icon _appIcon;

    private System.Windows.Forms.Timer? _reminderTimer;
    private bool _notificationActive = false;

    // Tray
    private NotifyIcon _trayIcon = null!;
    private ContextMenuStrip _trayMenu = null!;

    // Main UI controls
    private Label _phaseLabel = null!;
    private Label _timerLabel = null!;
    private Label _sessionCountLabel = null!;
    private TextBox _labelBox = null!;
    private Button _startPauseButton = null!;
    private Button _resetButton = null!;
    private Button _skipButton = null!;
    private Button _settingsButton = null!;
    private Panel _progressPanel = null!;
    private Panel _progressBar = null!;

    public MainForm()
    {
		_appIcon = LoadIcon();
        InitializeComponent();
        InitializeTray();
        WireTimerEvents();
        UpdateUI();
    }

    private void InitializeComponent()
    {
        Text = "Pomodoro";
		Icon = _appIcon;
        Size = new Size(420, 520);
        MinimumSize = new Size(380, 480);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(22, 22, 32);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10f);

        // Phase label
        _phaseLabel = new Label
        {
            Text = "WORK SESSION",
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.BottomCenter,
            ForeColor = Color.FromArgb(229, 57, 53),
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Padding = new Padding(0, 10, 0, 0)
        };

        // Big timer display
        _timerLabel = new Label
        {
            Text = "25:00",
            Dock = DockStyle.Top,
            Height = 120,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Light", 52f),
            ForeColor = Color.White
        };

        // Progress bar container
        _progressPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 6,
            BackColor = Color.FromArgb(45, 45, 58),
            Margin = new Padding(20, 0, 20, 0),
            Padding = new Padding(20, 0, 20, 0)
        };

        _progressBar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 0,
            BackColor = Color.FromArgb(229, 57, 53)
        };
        _progressPanel.Controls.Add(_progressBar);

        // Session count
        _sessionCountLabel = new Label
        {
            Text = "Sessions completed: 0",
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(130, 130, 155),
            Font = new Font("Segoe UI", 9f)
        };

        // Label / task entry
        var labelPrompt = new Label
        {
            Text = "What are you working on?",
            Dock = DockStyle.Top,
            Height = 24,
            TextAlign = ContentAlignment.BottomLeft,
            ForeColor = Color.FromArgb(130, 130, 155),
            Font = new Font("Segoe UI", 9f),
            Padding = new Padding(30, 0, 30, 0)
        };

        _labelBox = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 34,
            BackColor = Color.FromArgb(38, 38, 52),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10f),
            Margin = new Padding(30, 4, 30, 0)
        };
        _labelBox.TextChanged += (_, _) => _timer.SetLabel(_labelBox.Text);

        // Pad around textbox
        var labelPad = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.Transparent };
        labelPad.Controls.Add(_labelBox);
        _labelBox.Location = new Point(30, 8);
        _labelBox.Width = labelPad.Width - 60;
        labelPad.Resize += (_, _) => _labelBox.Width = labelPad.Width - 60;

        // Buttons row
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 54,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(20, 10, 20, 0),
            WrapContents = false
        };

        _startPauseButton = CreateButton("▶  Start", Color.FromArgb(229, 57, 53), 110);
        _startPauseButton.Click += OnStartPause;

        _resetButton = CreateButton("↺  Reset", Color.FromArgb(55, 55, 72), 90);
        _resetButton.Click += (_, _) => _timer.Reset();

        _skipButton = CreateButton("⏭  Skip", Color.FromArgb(55, 55, 72), 90);
        _skipButton.Click += (_, _) => _timer.SkipPhase();

        _settingsButton = CreateButton("⚙", Color.FromArgb(55, 55, 72), 44);
        _settingsButton.Click += OnOpenSettings;

        buttonPanel.Controls.AddRange([_startPauseButton, _resetButton, _skipButton, _settingsButton]);

        // Spacer between progress and session count
        var spacer = new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Color.Transparent };

        // Add controls in reverse order (Dock=Top stacks bottom-up)
        Controls.AddRange([
            buttonPanel,
            labelPad,
            labelPrompt,
            spacer,
            _sessionCountLabel,
            _progressPanel,
            _timerLabel,
            _phaseLabel
        ]);

        // Handle minimize to tray
        Resize += (_, _) =>
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                _trayIcon.Visible = true;
            }
        };

        FormClosing += (_, e) =>
        {
            _trayIcon.Visible = false;
        };
    }

    private void InitializeTray()
    {
        _trayMenu = new ContextMenuStrip();
        _trayMenu.BackColor = Color.FromArgb(38, 38, 52);
        _trayMenu.ForeColor = Color.White;
        _trayMenu.Renderer = new DarkMenuRenderer();

        var showItem = new ToolStripMenuItem("Show Pomodoro");
        showItem.Click += (_, _) => RestoreWindow();

        var startPauseItem = new ToolStripMenuItem("Start / Pause");
        startPauseItem.Click += OnStartPause;

        var resetItem = new ToolStripMenuItem("Reset");
        resetItem.Click += (_, _) => _timer.Reset();

        var separator = new ToolStripSeparator();

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => Application.Exit();

        _trayMenu.Items.AddRange([showItem, startPauseItem, resetItem, separator, exitItem]);

        _trayIcon = new NotifyIcon
        {
            Icon = _appIcon,
            ContextMenuStrip = _trayMenu,
            Text = "Pomodoro — 25:00 remaining",
            Visible = false
        };

        _trayIcon.DoubleClick += (_, _) => RestoreWindow();

        _trayIcon.BalloonTipClicked += (_, _) =>
        {
            _notificationActive = false;
            _reminderTimer?.Stop();
            RestoreWindow();
        };
    }

    private void WireTimerEvents()
    {
        _timer.Tick += UpdateUI;

        _timer.PhaseCompleted += phase =>
        {
            _alarm.Play();
            _notificationActive = true;

            var message = phase switch
            {
                PomodoroPhase.Work => "Work session complete! Time for a break.",
                PomodoroPhase.ShortBreak => "Break over — back to work!",
                PomodoroPhase.LongBreak => "Long break over — back to work!",
                _ => "Phase complete!"
            };

            _trayIcon.ShowBalloonTip(4000, "Pomodoro", message, ToolTipIcon.Info);

            // Start reminder timer
            _reminderTimer?.Stop();
            _reminderTimer = new System.Windows.Forms.Timer { Interval = 10000 };
            _reminderTimer.Tick += (_, _) =>
            {
                if (_notificationActive)
                    _alarm.Play();
                else
                    _reminderTimer?.Stop();
            };
            _reminderTimer.Start();

        };

        _timer.SessionLogged += _logger.Log;
    }

    private void UpdateUI()
    {
        if (InvokeRequired) { Invoke(UpdateUI); return; }

        var remaining = _timer.Remaining;
        var timeText = remaining.ToString(@"mm\:ss");

        // Timer display
        _timerLabel.Text = timeText;

        // Phase label + accent color
        var (phaseText, accentColor) = _timer.CurrentPhase switch
        {
            PomodoroPhase.Work => ("WORK SESSION", Color.FromArgb(229, 57, 53)),
            PomodoroPhase.ShortBreak => ("SHORT BREAK", Color.FromArgb(38, 166, 154)),
            PomodoroPhase.LongBreak => ("LONG BREAK", Color.FromArgb(66, 165, 245)),
            _ => ("WORK SESSION", Color.FromArgb(229, 57, 53))
        };

        _phaseLabel.Text = phaseText;
        _phaseLabel.ForeColor = accentColor;
        _progressBar.BackColor = accentColor;

        // Progress bar
        var progress = _timer.PhaseDuration.TotalSeconds > 0
            ? 1.0 - (remaining.TotalSeconds / _timer.PhaseDuration.TotalSeconds)
            : 0;
        _progressBar.Width = (int)(_progressPanel.Width * progress);

        // Session count
        _sessionCountLabel.Text = $"Sessions completed: {_timer.CompletedWorkSessions}";

        // Start/pause button
        _startPauseButton.Text = _timer.IsRunning ? "⏸  Pause" : "▶  Start";

        // Tray tooltip (visible when minimized)
        var label = string.IsNullOrWhiteSpace(_timer.CurrentLabel) ? "" : $" — {_timer.CurrentLabel}";
        var trayText = $"{phaseText}{label}\n{timeText} remaining";

        // NotifyIcon.Text has a 128-char limit
        _trayIcon.Text = trayText.Length > 127 ? trayText[..127] : trayText;
    }

    private void OnStartPause(object? sender, EventArgs e)
    {
        if (_timer.IsRunning) _timer.Pause();
        else _timer.Start();
        UpdateUI();
    }

    private void OnOpenSettings(object? sender, EventArgs e)
    {
        using var settings = new SettingsForm(_timer);
        if (settings.ShowDialog(this) == DialogResult.OK)
        {
            _timer.Reset();
            UpdateUI();
        }
    }

    private void RestoreWindow()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
        _trayIcon.Visible = false;
    }

    private static Button CreateButton(string text, Color backColor, int width) => new()
    {
        Text = text,
        Width = width,
        Height = 36,
        BackColor = backColor,
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        FlatAppearance = { BorderSize = 0 },
        Cursor = Cursors.Hand,
        Font = new Font("Segoe UI", 9.5f),
        Margin = new Padding(0, 0, 8, 0)
    };
	
	private static Icon LoadIcon()
    {
        // Load from embedded resource (compiled into the exe)
        var assembly = typeof(MainForm).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("pomodoro.ico"));

        if (resourceName is not null)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            return new Icon(stream);
        }

        // Fallback if resource is somehow missing
        return SystemIcons.Application;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _alarm.Dispose();
            _trayIcon.Dispose();
            _trayMenu.Dispose();
        }
        base.Dispose(disposing);
    }
}

/// <summary>Dark-themed renderer for the tray context menu.</summary>
file sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
{
    public DarkMenuRenderer() : base(new DarkColorTable()) { }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        var color = e.Item.Selected
            ? Color.FromArgb(60, 60, 80)
            : Color.FromArgb(38, 38, 52);
        e.Graphics.FillRectangle(new SolidBrush(color), e.Item.ContentRectangle);
    }
}

file sealed class DarkColorTable : ProfessionalColorTable
{
    public override Color MenuBorder => Color.FromArgb(60, 60, 80);
    public override Color ToolStripDropDownBackground => Color.FromArgb(38, 38, 52);
    public override Color ImageMarginGradientBegin => Color.FromArgb(38, 38, 52);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(38, 38, 52);
    public override Color ImageMarginGradientEnd => Color.FromArgb(38, 38, 52);
    public override Color SeparatorDark => Color.FromArgb(60, 60, 80);
    public override Color SeparatorLight => Color.FromArgb(60, 60, 80);
}
