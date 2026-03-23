using System;
using System.Drawing;
using System.Windows.Forms;

namespace NtClock
{
    internal sealed class SettingsForm : Form
    {
        private readonly CheckBox _showTitleBar = new() { Text = "Show title bar" };
        private readonly CheckBox _alwaysOnTop = new() { Text = "Always on top" };
        private readonly CheckBox _showSeconds = new() { Text = "Show seconds" };
        private readonly CheckBox _showDigital = new() { Text = "Show digital time" };
        private readonly CheckBox _showDate = new() { Text = "Show date" };
        private readonly CheckBox _use24Hour = new() { Text = "Use 24-hour format" };
        private readonly CheckBox _hideToTray = new() { Text = "Hide to tray on close" };
        private readonly CheckBox _startMinimized = new() { Text = "Start minimized to tray" };
        private readonly CheckBox _smoothSeconds = new() { Text = "Smooth second hand" };
        private readonly CheckBox _alarmEnabled = new() { Text = "Enable daily alarm" };
        private readonly NumericUpDown _alarmHour = new() { Minimum = 0, Maximum = 23, Width = 50 };
        private readonly NumericUpDown _alarmMinute = new() { Minimum = 0, Maximum = 59, Width = 50 };
        private readonly Button _ok = new() { Text = "OK", DialogResult = DialogResult.OK };
        private readonly Button _cancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

        public SettingsForm(NtSettings settings)
        {
            Text = "Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(280, 355);
            BackColor = SystemColors.Control;
            Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);

            int y = 12;
            foreach (Control c in new Control[]
            {
                _showTitleBar, _alwaysOnTop, _showSeconds, _showDigital, _showDate,
                _use24Hour, _hideToTray, _startMinimized, _smoothSeconds, _alarmEnabled
            })
            {
                c.Location = new Point(12, y);
                c.AutoSize = true;
                Controls.Add(c);
                y += 24;
            }

            var alarmLabel = new Label
            {
                AutoSize = true,
                Text = "Alarm time:",
                Location = new Point(32, y + 4)
            };
            Controls.Add(alarmLabel);

            _alarmHour.Location = new Point(100, y);
            _alarmMinute.Location = new Point(160, y);
            Controls.Add(_alarmHour);
            Controls.Add(_alarmMinute);

            var colon = new Label
            {
                AutoSize = true,
                Text = ":",
                Location = new Point(151, y + 4)
            };
            Controls.Add(colon);

            _ok.Location = new Point(110, 318);
            _cancel.Location = new Point(190, 318);
            Controls.Add(_ok);
            Controls.Add(_cancel);

            AcceptButton = _ok;
            CancelButton = _cancel;

            _showTitleBar.Checked = settings.ShowTitleBar;
            _alwaysOnTop.Checked = settings.AlwaysOnTop;
            _showSeconds.Checked = settings.ShowSeconds;
            _showDigital.Checked = settings.ShowDigitalTime;
            _showDate.Checked = settings.ShowDate;
            _use24Hour.Checked = settings.Use24Hour;
            _hideToTray.Checked = settings.HideToTrayOnClose;
            _startMinimized.Checked = settings.StartMinimizedToTray;
            _smoothSeconds.Checked = settings.SmoothSecondHand;
            _alarmEnabled.Checked = settings.AlarmEnabled;
            _alarmHour.Value = settings.AlarmHour;
            _alarmMinute.Value = settings.AlarmMinute;

            _showDigital.CheckedChanged += (_, __) => UpdateEnabledState();
            _showSeconds.CheckedChanged += (_, __) => UpdateEnabledState();
            _hideToTray.CheckedChanged += (_, __) => UpdateEnabledState();
            UpdateEnabledState();
        }

        public void ApplyTo(NtSettings settings)
        {
            settings.ShowTitleBar = _showTitleBar.Checked;
            settings.AlwaysOnTop = _alwaysOnTop.Checked;
            settings.ShowSeconds = _showSeconds.Checked;
            settings.ShowDigitalTime = _showDigital.Checked;
            settings.ShowDate = _showDate.Checked;
            settings.Use24Hour = _use24Hour.Checked;
            settings.HideToTrayOnClose = _hideToTray.Checked;
            settings.StartMinimizedToTray = _startMinimized.Checked && _hideToTray.Checked;
            settings.SmoothSecondHand = _showSeconds.Checked && _smoothSeconds.Checked;
            settings.AlarmEnabled = _alarmEnabled.Checked;
            settings.AlarmHour = (int)_alarmHour.Value;
            settings.AlarmMinute = (int)_alarmMinute.Value;
        }

        private void UpdateEnabledState()
        {
            _use24Hour.Enabled = _showDigital.Checked;
            _showDate.Enabled = _showDigital.Checked;
            _smoothSeconds.Enabled = _showSeconds.Checked;
            _startMinimized.Enabled = _hideToTray.Checked;
            if (!_hideToTray.Checked)
            {
                _startMinimized.Checked = false;
            }
        }
    }
}
