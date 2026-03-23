namespace WorkTimer;

internal sealed class TimerApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private Settings _settings;
    private DateTime _startTime;
    private int _tickCount;
    private TimeSpan _pausedDuration;
    private DateTime? _pauseStart;

    public TimerApplicationContext()
    {
        _settings = Settings.Load();
        _startTime = DateTime.Now;

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadEmbeddedIcon(),
            Text = "WorkTimer — 00:00:00",
            Visible = true,
            ContextMenuStrip = BuildContextMenu(),
        };

        _notifyIcon.ShowBalloonTip(
            2000,
            "WorkTimer",
            $"Timer started at {_startTime:h:mm:ss tt}",
            ToolTipIcon.Info);

        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        NativeMethods.PreventSleep();
    }

    private static Icon LoadEmbeddedIcon()
    {
        var exe = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(exe) && File.Exists(exe))
            return Icon.ExtractAssociatedIcon(exe) ?? SystemIcons.Application;

        return SystemIcons.Application;
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();

        var elapsedItem = new ToolStripMenuItem("Elapsed: 00:00:00") { Enabled = false };
        elapsedItem.Name = "elapsed";

        var startedItem = new ToolStripMenuItem($"Started: {_startTime:h:mm:ss tt}") { Enabled = false };
        startedItem.Name = "started";

        var pauseItem = new ToolStripMenuItem("Pause") { Name = "pause" };
        pauseItem.Click += OnPauseResume;

        var resetItem = new ToolStripMenuItem("Reset Timer");
        resetItem.Click += OnReset;

        var optionsItem = new ToolStripMenuItem("Options...");
        optionsItem.Click += OnOptions;

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += OnExit;

        menu.Items.Add(elapsedItem);
        menu.Items.Add(startedItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(pauseItem);
        menu.Items.Add(resetItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(optionsItem);
        menu.Items.Add(exitItem);

        return menu;
    }

    private TimeSpan GetElapsed()
    {
        var elapsed = DateTime.Now - _startTime - _pausedDuration;
        if (_pauseStart.HasValue)
            elapsed -= DateTime.Now - _pauseStart.Value;
        return elapsed;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var full = GetElapsed().ToString(@"hh\:mm\:ss");
        var paused = _pauseStart.HasValue;

        _notifyIcon.Text = paused
            ? $"WorkTimer \u2014 {full} (Paused)"
            : $"WorkTimer \u2014 {full}";

        if (_notifyIcon.ContextMenuStrip?.Items["elapsed"] is ToolStripMenuItem item)
        {
            item.Text = $"Elapsed: {full}";
        }

        if (!paused)
        {
            _tickCount++;
            if (_tickCount >= _settings.IntervalSeconds)
            {
                _tickCount = 0;
                NativeMethods.SimulateKeyPress(_settings.GetVirtualKeyCode());
            }
        }
    }

    private void OnPauseResume(object? sender, EventArgs e)
    {
        if (_pauseStart.HasValue)
        {
            // Resume
            _pausedDuration += DateTime.Now - _pauseStart.Value;
            _pauseStart = null;
            NativeMethods.PreventSleep();

            if (_notifyIcon.ContextMenuStrip?.Items["pause"] is ToolStripMenuItem item)
                item.Text = "Pause";
        }
        else
        {
            // Pause
            _pauseStart = DateTime.Now;
            NativeMethods.AllowSleep();

            if (_notifyIcon.ContextMenuStrip?.Items["pause"] is ToolStripMenuItem item)
                item.Text = "Resume";
        }
    }

    private void OnReset(object? sender, EventArgs e)
    {
        _startTime = DateTime.Now;
        _pausedDuration = TimeSpan.Zero;
        _pauseStart = null;
        _tickCount = 0;
        NativeMethods.PreventSleep();

        if (_notifyIcon.ContextMenuStrip?.Items["started"] is ToolStripMenuItem item)
            item.Text = $"Started: {_startTime:h:mm:ss tt}";

        if (_notifyIcon.ContextMenuStrip?.Items["pause"] is ToolStripMenuItem pauseItem)
            pauseItem.Text = "Pause";

        _notifyIcon.ShowBalloonTip(
            1500,
            "WorkTimer",
            $"Timer reset at {_startTime:h:mm:ss tt}",
            ToolTipIcon.Info);
    }

    private void OnOptions(object? sender, EventArgs e)
    {
        using var form = new OptionsForm(_settings);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _settings.VirtualKey = form.SelectedKey;
            _settings.IntervalSeconds = form.SelectedInterval;
            _settings.Save();
            _tickCount = 0;
        }
    }

    private void OnExit(object? sender, EventArgs e)
    {
        _timer.Stop();
        _timer.Dispose();
        NativeMethods.AllowSleep();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            NativeMethods.AllowSleep();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
