using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using WinFormsTimer = System.Windows.Forms.Timer;

namespace NtClock
{
    internal sealed class ClockForm : Form
    {
        private readonly NtSettings _settings;
        private readonly AnalogClockControl _clock;
        private readonly Label _digitalTime;
        private readonly Label _digitalDate;
        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenuStrip _trayMenu;
        private readonly ContextMenuStrip _windowMenu;
        private readonly WinFormsTimer _uiTimer;
        private readonly WinFormsTimer _alarmTimer;
        private readonly WinFormsTimer _alignTimer;
        private bool _allowRealClose;

        public ClockForm()
        {
            _settings = NtSettings.Load();

            Text = "Clock";
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = SystemColors.Control;
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);
            MaximizeBox = false;
            MinimizeBox = true;
            ShowInTaskbar = true;

            _clock = new AnalogClockControl
            {
                Location = new Point(8, 8),
                Size = new Size(220, 220),
                BackColor = SystemColors.Control
            };

            _digitalTime = new Label
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(8, 236),
                Size = new Size(220, 18),
                BackColor = SystemColors.Control,
                ForeColor = SystemColors.ControlText,
                Font = new Font("MS Sans Serif", 8.25f, FontStyle.Bold),
                Visible = false
            };

            _digitalDate = new Label
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(8, 256),
                Size = new Size(220, 16),
                BackColor = SystemColors.Control,
                ForeColor = SystemColors.ControlText,
                Font = new Font("MS Sans Serif", 8.25f, FontStyle.Regular),
                Visible = false
            };

            Controls.Add(_clock);
            Controls.Add(_digitalTime);
            Controls.Add(_digitalDate);

            _trayMenu = new ContextMenuStrip();
            _windowMenu = new ContextMenuStrip();
            BuildMenus();
            ContextMenuStrip = _windowMenu;

            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Clock",
                Visible = true,
                ContextMenuStrip = _trayMenu
            };
            _trayIcon.DoubleClick += (_, __) => RestoreFromTray();

            _uiTimer = new WinFormsTimer();
            _uiTimer.Tick += (_, __) => UpdateClock();

            _alarmTimer = new WinFormsTimer { Interval = 1000 };
            _alarmTimer.Tick += (_, __) => CheckAlarm();
            _alarmTimer.Start();

            _alignTimer = new WinFormsTimer();
            _alignTimer.Tick += AlignTimerTick;

            ApplySettings();
            RestorePosition();

            Load += (_, __) =>
            {
                if (_settings.StartMinimizedToTray && _settings.HideToTrayOnClose)
                {
                    HideToTray();
                }
                else
                {
                    UpdateClock();
                }
            };
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowRealClose && _settings.HideToTrayOnClose)
            {
                e.Cancel = true;
                HideToTray();
                return;
            }

            SavePosition();
            _settings.Save();
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _uiTimer?.Stop();
                _alarmTimer?.Stop();
                _alignTimer?.Stop();
                _uiTimer?.Dispose();
                _alarmTimer?.Dispose();
                _alignTimer?.Dispose();
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayMenu.Dispose();
                _windowMenu.Dispose();
            }

            base.Dispose(disposing);
        }

        private void ApplySettings()
        {
            FormBorderStyle = _settings.ShowTitleBar ? FormBorderStyle.FixedSingle : FormBorderStyle.None;
            TopMost = _settings.AlwaysOnTop;

            _clock.ShowSeconds = _settings.ShowSeconds;
            _clock.SmoothSecondHand = _settings.ShowSeconds && _settings.SmoothSecondHand;

            _digitalTime.Visible = _settings.ShowDigitalTime;
            _digitalDate.Visible = _settings.ShowDigitalTime && _settings.ShowDate;

            ClientSize = _settings.ShowDigitalTime
                ? new Size(236, _settings.ShowDate ? 280 : 262)
                : new Size(236, 236);

            UpdateUiTimerMode();
        }

        private void UpdateUiTimerMode()
        {
            _alignTimer.Stop();
            _uiTimer.Stop();

            if (_settings.ShowSeconds && _settings.SmoothSecondHand)
            {
                _uiTimer.Interval = 50;
                _uiTimer.Start();
                return;
            }

            int msToNext = 1000 - DateTime.Now.Millisecond;
            if (msToNext < 10)
            {
                msToNext = 10;
            }

            _alignTimer.Interval = msToNext;
            _alignTimer.Start();
        }

        private void AlignTimerTick(object sender, EventArgs e)
        {
            _alignTimer.Stop();
            UpdateClock();
            _uiTimer.Interval = 1000;
            _uiTimer.Start();
        }

        private void UpdateClock()
        {
            var now = DateTime.Now;
            _clock.CurrentTime = now;
            _clock.Invalidate();

            if (_settings.ShowDigitalTime)
            {
                _digitalTime.Text = _settings.Use24Hour
                    ? now.ToString(_settings.ShowSeconds ? "HH:mm:ss" : "HH:mm")
                    : now.ToString(_settings.ShowSeconds ? "hh:mm:ss tt" : "hh:mm tt");
                _digitalDate.Text = now.ToString("ddd dd.MM.yyyy");
            }
        }

        private void CheckAlarm()
        {
            if (!_settings.AlarmEnabled)
            {
                return;
            }

            var now = DateTime.Now;
            if (now.Hour != _settings.AlarmHour || now.Minute != _settings.AlarmMinute)
            {
                return;
            }

            string stamp = now.ToString("yyyyMMddHHmm");
            if (stamp == _settings.LastAlarmStamp)
            {
                return;
            }

            _settings.LastAlarmStamp = stamp;
            _settings.Save();
            FireAlert("Alarm", $"It is {now:HH:mm}.");
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
                _trayIcon.BalloonTipTitle = title;
                _trayIcon.BalloonTipText = message;
                _trayIcon.ShowBalloonTip(3000);
                return;
            }

            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void HideToTray()
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
            UpdateClock();
        }

        private void OpenSettings()
        {
            using var dialog = new SettingsForm(_settings);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                dialog.ApplyTo(_settings);
                _settings.Save();
                ApplySettings();
                UpdateClock();
                BuildMenus();
            }
        }

        private void ExitClock()
        {
            _allowRealClose = true;
            Close();
        }

        private void ToggleDigital()
        {
            _settings.ShowDigitalTime = !_settings.ShowDigitalTime;
            if (!_settings.ShowDigitalTime)
            {
                _settings.ShowDate = false;
            }
            _settings.Save();
            ApplySettings();
            UpdateClock();
            BuildMenus();
        }

        private void ToggleDate()
        {
            if (!_settings.ShowDigitalTime)
            {
                _settings.ShowDigitalTime = true;
            }
            _settings.ShowDate = !_settings.ShowDate;
            _settings.Save();
            ApplySettings();
            UpdateClock();
            BuildMenus();
        }

        private void ToggleAlwaysOnTop()
        {
            _settings.AlwaysOnTop = !_settings.AlwaysOnTop;
            _settings.Save();
            ApplySettings();
            BuildMenus();
        }

        private void ToggleTitleBar()
        {
            _settings.ShowTitleBar = !_settings.ShowTitleBar;
            _settings.Save();
            ApplySettings();
            BuildMenus();
        }

        private void BuildMenus()
        {
            _trayMenu.Items.Clear();
            _windowMenu.Items.Clear();

            AddMenuItems(_windowMenu, includeHide: true);
            AddMenuItems(_trayMenu, includeHide: false);
        }

        private void AddMenuItems(ContextMenuStrip menu, bool includeHide)
        {
            var showItem = new ToolStripMenuItem("Show") { Enabled = !Visible || !ShowInTaskbar };
            showItem.Click += (_, __) => RestoreFromTray();
            menu.Items.Add(showItem);

            if (includeHide)
            {
                var hideItem = new ToolStripMenuItem("Hide") { Enabled = _settings.HideToTrayOnClose };
                hideItem.Click += (_, __) => HideToTray();
                menu.Items.Add(hideItem);
            }

            menu.Items.Add(new ToolStripSeparator());

            var digitalItem = new ToolStripMenuItem("Digital time") { Checked = _settings.ShowDigitalTime };
            digitalItem.Click += (_, __) => ToggleDigital();
            menu.Items.Add(digitalItem);

            var dateItem = new ToolStripMenuItem("Date") { Checked = _settings.ShowDate, Enabled = _settings.ShowDigitalTime };
            dateItem.Click += (_, __) => ToggleDate();
            menu.Items.Add(dateItem);

            var topItem = new ToolStripMenuItem("Always on top") { Checked = _settings.AlwaysOnTop };
            topItem.Click += (_, __) => ToggleAlwaysOnTop();
            menu.Items.Add(topItem);

            var frameItem = new ToolStripMenuItem("Title bar") { Checked = _settings.ShowTitleBar };
            frameItem.Click += (_, __) => ToggleTitleBar();
            menu.Items.Add(frameItem);

            menu.Items.Add(new ToolStripSeparator());

            var settingsItem = new ToolStripMenuItem("Settings");
            settingsItem.Click += (_, __) => OpenSettings();
            menu.Items.Add(settingsItem);

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (_, __) => ExitClock();
            menu.Items.Add(exitItem);
        }

        private void RestorePosition()
        {
            if (_settings.Left < 0 || _settings.Top < 0)
            {
                return;
            }

            StartPosition = FormStartPosition.Manual;
            Location = new Point(_settings.Left, _settings.Top);
        }

        private void SavePosition()
        {
            if (WindowState != FormWindowState.Normal)
            {
                return;
            }

            _settings.Left = Left;
            _settings.Top = Top;
        }
    }
}
