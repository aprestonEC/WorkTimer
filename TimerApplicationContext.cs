namespace WorkTimer;

internal sealed class TimerApplicationContext : ApplicationContext
{
    private const int InputIntervalSeconds = 60;

    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _timer;
    private DateTime _startTime;
    private int _tickCount;

    public TimerApplicationContext()
    {
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

        var resetItem = new ToolStripMenuItem("Reset Timer");
        resetItem.Click += OnReset;

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += OnExit;

        menu.Items.Add(elapsedItem);
        menu.Items.Add(startedItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(resetItem);
        menu.Items.Add(exitItem);

        return menu;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var elapsed = DateTime.Now - _startTime;
        var full = elapsed.ToString(@"hh\:mm\:ss");

        _notifyIcon.Text = $"WorkTimer \u2014 {full}";

        if (_notifyIcon.ContextMenuStrip?.Items["elapsed"] is ToolStripMenuItem item)
        {
            item.Text = $"Elapsed: {full}";
        }

        _tickCount++;
        if (_tickCount >= InputIntervalSeconds)
        {
            _tickCount = 0;
            NativeMethods.SimulateF16Press();
        }
    }

    private void OnReset(object? sender, EventArgs e)
    {
        _startTime = DateTime.Now;

        if (_notifyIcon.ContextMenuStrip?.Items["started"] is ToolStripMenuItem item)
        {
            item.Text = $"Started: {_startTime:h:mm:ss tt}";
        }

        _notifyIcon.ShowBalloonTip(
            1500,
            "WorkTimer",
            $"Timer reset at {_startTime:h:mm:ss tt}",
            ToolTipIcon.Info);
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
