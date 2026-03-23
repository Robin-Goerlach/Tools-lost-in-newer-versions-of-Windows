using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace NtClock;

public sealed class ClockForm : Form
{
    private readonly System.Windows.Forms.Timer _uiTimer = new();
    private readonly System.Windows.Forms.Timer _alarmTimer = new() { Interval = 1000 };
    private readonly System.Windows.Forms.Timer _countdownTimer = new() { Interval = 1000 };
    private System.Windows.Forms.Timer? _alignOneShot;
    private readonly System.Windows.Forms.Timer _saveBoundsDebounce = new() { Interval = 500 };

    private readonly AnalogClockControl analogClock = new();
    private readonly DigitalTerminalControl digitalTerminal = new();
    private readonly DigitalClassicControl digitalClassic = new();
    private readonly Label lblDate = new();

    private readonly Panel outerPanel = new();
    private readonly Panel innerPanel = new();

    private readonly NotifyIcon trayIcon = new();
    private readonly ContextMenuStrip trayMenu = new();
    private readonly ContextMenuStrip formMenu = new();

    private bool _allowExit = false;

    public ClockForm()
    {
        Text = "Clock";
        Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);
        BackColor = SystemColors.Control;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = true;
        StartPosition = FormStartPosition.CenterScreen;

        outerPanel.BorderStyle = BorderStyle.Fixed3D;
        outerPanel.BackColor = SystemColors.Control;
        outerPanel.Location = new Point(8, 8);
        Controls.Add(outerPanel);

        innerPanel.BorderStyle = BorderStyle.Fixed3D;
        innerPanel.BackColor = SystemColors.Control;
        innerPanel.Location = new Point(8, 8);
        outerPanel.Controls.Add(innerPanel);

        innerPanel.Controls.Add(analogClock);
        innerPanel.Controls.Add(digitalTerminal);
        innerPanel.Controls.Add(digitalClassic);

        lblDate.AutoSize = false;
        lblDate.TextAlign = ContentAlignment.MiddleCenter;
        lblDate.Size = new Size(164, 18);
        lblDate.ForeColor = SystemColors.ControlText;
        lblDate.BackColor = SystemColors.Control;
        innerPanel.Controls.Add(lblDate);

        trayIcon.Icon = SystemIcons.Application;
        trayIcon.Text = "Clock";
        trayIcon.Visible = true;
        trayIcon.ContextMenuStrip = trayMenu;
        trayIcon.DoubleClick += (_, __) => RestoreFromTray();

        _uiTimer.Tick += (_, __) => TickUi();
        _alarmTimer.Tick += (_, __) => CheckAlarm();
        _countdownTimer.Tick += (_, __) => CheckCountdown();

        _saveBoundsDebounce.Tick += (_, __) =>
        {
            _saveBoundsDebounce.Stop();
            SaveWindowBounds();
        };

        Load += (_, __) =>
        {
            RestoreWindowBounds();
            ApplySettings();
            LayoutNtTypical();

            if (AppSettings.Current.StartMinimizedToTray && AppSettings.Current.MinimizeToTray)
            {
                MinimizeToTray();
            }
            else
            {
                TickUi();
            }

            _alarmTimer.Start();
            _countdownTimer.Start();
        };

        Move += (_, __) => DebounceSaveBounds();
        ResizeEnd += (_, __) => DebounceSaveBounds();

        BuildContextMenus();
        ContextMenuStrip = formMenu;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_allowExit)
        {
            e.Cancel = true;
            if (AppSettings.Current.MinimizeToTray)
            {
                MinimizeToTray();
            }
            else
            {
                WindowState = FormWindowState.Minimized;
            }
            return;
        }

        SaveWindowBounds();
        AppSettings.Save();
        base.OnFormClosing(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized && AppSettings.Current.MinimizeToTray)
        {
            MinimizeToTray();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _alignOneShot?.Stop();
            _alignOneShot?.Dispose();
            _uiTimer.Stop();
            _alarmTimer.Stop();
            _countdownTimer.Stop();
            _saveBoundsDebounce.Stop();

            trayIcon.Visible = false;
            trayIcon.Dispose();
            trayMenu.Dispose();
            formMenu.Dispose();

            _uiTimer.Dispose();
            _alarmTimer.Dispose();
            _countdownTimer.Dispose();
            _saveBoundsDebounce.Dispose();
        }

        base.Dispose(disposing);
    }

    private void DebounceSaveBounds()
    {
        if (!Visible || WindowState != FormWindowState.Normal)
        {
            return;
        }

        _saveBoundsDebounce.Stop();
        _saveBoundsDebounce.Start();
    }

    private void SaveWindowBounds()
    {
        if (!Visible || WindowState != FormWindowState.Normal)
        {
            return;
        }

        var s = AppSettings.Current;
        s.WinX = Left;
        s.WinY = Top;
        s.WinW = Width;
        s.WinH = Height;
    }

    private void RestoreWindowBounds()
    {
        var s = AppSettings.Current;
        if (s.WinX < 0 || s.WinY < 0 || s.WinW < 0 || s.WinH < 0)
        {
            return;
        }

        var rect = new Rectangle(s.WinX, s.WinY, s.WinW, s.WinH);
        foreach (Screen scr in Screen.AllScreens)
        {
            if (scr.WorkingArea.IntersectsWith(rect))
            {
                StartPosition = FormStartPosition.Manual;
                Bounds = rect;
                return;
            }
        }
    }

    private void ApplySettings()
    {
        var s = AppSettings.Current;
        TopMost = s.AlwaysOnTop;
        analogClock.ShowSeconds = s.ShowSeconds;
        analogClock.UseRedSecondHand = s.RedSecondHand;

        bool smoothEffective = s.ShowSeconds && s.SmoothSecondHand;
        analogClock.SmoothSecondHand = smoothEffective;

        digitalTerminal.Use24Hour = s.Use24Hour;
        digitalTerminal.ShowSeconds = s.ShowSeconds;
        digitalClassic.Use24Hour = s.Use24Hour;
        digitalClassic.ShowSeconds = s.ShowSeconds;

        ApplyTimingPolicy(smoothEffective);
    }

    private void ApplyTimingPolicy(bool smoothEffective)
    {
        _alignOneShot?.Stop();
        _alignOneShot?.Dispose();
        _alignOneShot = null;

        if (smoothEffective)
        {
            _uiTimer.Stop();
            _uiTimer.Interval = 50;
            _uiTimer.Start();
            return;
        }

        _uiTimer.Stop();
        _uiTimer.Interval = 1000;

        int msToNext = 1000 - DateTime.Now.Millisecond;
        if (msToNext < 10)
        {
            msToNext = 10;
        }

        _alignOneShot = new System.Windows.Forms.Timer { Interval = msToNext };
        _alignOneShot.Tick += (_, __) =>
        {
            _alignOneShot?.Stop();
            _alignOneShot?.Dispose();
            _alignOneShot = null;

            TickUi();
            _uiTimer.Start();
        };
        _alignOneShot.Start();
    }

    private void LayoutNtTypical()
    {
        int pad = 10;
        int y = pad;

        analogClock.Location = new Point(pad, y);
        y = analogClock.Bottom + 8;

        digitalTerminal.Location = new Point(pad, y);
        digitalClassic.Location = new Point(pad, y);

        switch (AppSettings.Current.DigitalStyle)
        {
            case DigitalStyle.Off:
                digitalTerminal.Visible = false;
                digitalClassic.Visible = false;
                break;
            case DigitalStyle.Terminal:
                digitalTerminal.Visible = true;
                digitalClassic.Visible = false;
                y = digitalTerminal.Bottom + 6;
                break;
            default:
                digitalTerminal.Visible = false;
                digitalClassic.Visible = true;
                y = digitalClassic.Bottom + 6;
                break;
        }

        lblDate.Location = new Point(pad, y);
        lblDate.Visible = AppSettings.Current.ShowDate;
        y = lblDate.Visible ? (lblDate.Bottom + 10) : (y + 10);

        innerPanel.Size = new Size(184, y);
        outerPanel.Size = new Size(innerPanel.Width + 16, innerPanel.Height + 16);
        ClientSize = new Size(outerPanel.Right + 8, outerPanel.Bottom + 8);
    }

    private void TickUi()
    {
        DateTime now = DateTime.Now;
        analogClock.Time = now;
        analogClock.Invalidate();

        switch (AppSettings.Current.DigitalStyle)
        {
            case DigitalStyle.Terminal:
                digitalTerminal.Time = now;
                digitalTerminal.Invalidate();
                break;
            case DigitalStyle.Classic:
                digitalClassic.Time = now;
                digitalClassic.Invalidate();
                break;
        }

        if (AppSettings.Current.ShowDate)
        {
            lblDate.Text = now.ToString("ddd dd.MM.yyyy");
        }

        LayoutNtTypical();
    }

    private void BuildContextMenus()
    {
        formMenu.Items.Clear();
        trayMenu.Items.Clear();

        var showItem = new ToolStripMenuItem("Show");
        showItem.Click += (_, __) => RestoreFromTray();

        var optionsItem = new ToolStripMenuItem("Options...");
        optionsItem.Click += (_, __) => ShowOptions();

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, __) => ExitApp();

        formMenu.Items.Add(showItem);
        formMenu.Items.Add(optionsItem);
        formMenu.Items.Add(new ToolStripSeparator());
        formMenu.Items.Add(exitItem);

        var trayShow = new ToolStripMenuItem("Show");
        trayShow.Click += (_, __) => RestoreFromTray();
        var trayOptions = new ToolStripMenuItem("Options...");
        trayOptions.Click += (_, __) => ShowOptions();
        var trayExit = new ToolStripMenuItem("Exit");
        trayExit.Click += (_, __) => ExitApp();

        trayMenu.Items.Add(trayShow);
        trayMenu.Items.Add(trayOptions);
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add(trayExit);
    }

    private void ShowOptions()
    {
        using var dlg = new SettingsForm();
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            AppSettings.Save();
            ApplySettings();
            TickUi();
        }
    }

    private void MinimizeToTray()
    {
        Hide();
        ShowInTaskbar = false;
    }

    private void RestoreFromTray()
    {
        Show();
        ShowInTaskbar = true;
        WindowState = FormWindowState.Normal;
        Activate();
        TickUi();
    }

    private void ExitApp()
    {
        _allowExit = true;
        trayIcon.Visible = false;
        Close();
    }

    private void CheckAlarm()
    {
        var s = AppSettings.Current;
        if (!s.AlarmEnabled)
        {
            return;
        }

        if (!TimeSpan.TryParse(s.AlarmTimeHHmm, out var alarmTs))
        {
            return;
        }

        DateTime now = DateTime.Now;
        if (now.Hour == alarmTs.Hours && now.Minute == alarmTs.Minutes)
        {
            string stamp = now.ToString("yyyyMMddHHmm");
            if (stamp == s.AlarmLastFiredStamp)
            {
                return;
            }

            s.AlarmLastFiredStamp = stamp;
            AppSettings.Save();
            FireAlert("Alarm", $"Alarm time reached: {s.AlarmTimeHHmm}");
        }
    }

    private void CheckCountdown()
    {
        long endTicks = AppSettings.Current.CountdownEndUtcTicks;
        if (endTicks <= 0)
        {
            return;
        }

        DateTime end = new(endTicks, DateTimeKind.Utc);
        TimeSpan remaining = end - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            AppSettings.Current.CountdownEndUtcTicks = 0;
            AppSettings.Save();
            FireAlert("Timer", "Countdown finished.");
        }
    }

    private void FireAlert(string title, string message)
    {
        try
        {
            SystemSounds.Exclamation.Play();
        }
        catch
        {
        }

        if (!Visible)
        {
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipText = message;
            trayIcon.ShowBalloonTip(3000);
            return;
        }

        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
