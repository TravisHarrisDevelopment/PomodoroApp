namespace PomodoroApp;

public class SettingsForm : Form
{
    private readonly PomodoroTimer _timer;

    private NumericUpDown _workMinutes = null!;
    private NumericUpDown _shortBreakMinutes = null!;
    private NumericUpDown _longBreakMinutes = null!;
    private NumericUpDown _sessionsBeforeLong = null!;
    private Button _okButton = null!;
    private Button _cancelButton = null!;

    public SettingsForm(PomodoroTimer timer)
    {
        _timer = timer;
        InitializeComponent();
        LoadCurrentSettings();
    }

    private void InitializeComponent()
    {
        Text = "Settings";
        Size = new Size(320, 280);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(30, 30, 40);
        ForeColor = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            RowCount = 5,
            ColumnCount = 2
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        _workMinutes = CreateSpinner(1, 120);
        _shortBreakMinutes = CreateSpinner(1, 60);
        _longBreakMinutes = CreateSpinner(1, 60);
        _sessionsBeforeLong = CreateSpinner(1, 10);

        AddRow(layout, 0, "Work duration (minutes):", _workMinutes);
        AddRow(layout, 1, "Short break (minutes):", _shortBreakMinutes);
        AddRow(layout, 2, "Long break (minutes):", _longBreakMinutes);
        AddRow(layout, 3, "Sessions before long break:", _sessionsBeforeLong);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };

        _cancelButton = new Button { Text = "Cancel", Width = 80 };
        _cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;
        StyleButton(_cancelButton, secondary: true);

        _okButton = new Button { Text = "Save", Width = 80 };
        _okButton.Click += OnSave;
        StyleButton(_okButton);

        buttonPanel.Controls.AddRange([_cancelButton, _okButton]);
        layout.Controls.Add(buttonPanel, 0, 4);
        layout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(layout);
    }

    private static NumericUpDown CreateSpinner(int min, int max) => new()
    {
        Minimum = min,
        Maximum = max,
        Width = 60,
        BackColor = Color.FromArgb(45, 45, 58),
        ForeColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle
    };

    private static void AddRow(TableLayoutPanel layout, int row, string label, Control control)
    {
        var lbl = new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(180, 180, 200)
        };
        layout.Controls.Add(lbl, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private static void StyleButton(Button btn, bool secondary = false)
    {
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = secondary
            ? Color.FromArgb(80, 80, 100)
            : Color.FromArgb(229, 57, 53);
        btn.BackColor = secondary
            ? Color.FromArgb(45, 45, 58)
            : Color.FromArgb(229, 57, 53);
        btn.ForeColor = Color.White;
        btn.Cursor = Cursors.Hand;
    }

    private void LoadCurrentSettings()
    {
        _workMinutes.Value = (int)_timer.WorkDuration.TotalMinutes;
        _shortBreakMinutes.Value = (int)_timer.ShortBreakDuration.TotalMinutes;
        _longBreakMinutes.Value = (int)_timer.LongBreakDuration.TotalMinutes;
        _sessionsBeforeLong.Value = _timer.SessionsBeforeLongBreak;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _timer.WorkDuration = TimeSpan.FromMinutes((double)_workMinutes.Value);
        _timer.ShortBreakDuration = TimeSpan.FromMinutes((double)_shortBreakMinutes.Value);
        _timer.LongBreakDuration = TimeSpan.FromMinutes((double)_longBreakMinutes.Value);
        _timer.SessionsBeforeLongBreak = (int)_sessionsBeforeLong.Value;
        DialogResult = DialogResult.OK;
    }
}
