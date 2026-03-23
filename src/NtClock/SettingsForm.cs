using System;
using System.Drawing;
using System.Windows.Forms;

namespace NtClock;

public sealed class SettingsForm : Form
{
    private readonly System.Windows.Forms.Timer _ui = new() { Interval = 1000 };

    private readonly ComboBox cmbDigitalStyle = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox cbSeconds = new() { Text = "Show seconds" };
    private readonly CheckBox cbRedSecond = new() { Text = "Red second hand" };
    private readonly CheckBox cbSmoothSecond = new() { Text = "Smooth second hand" };
    private readonly CheckBox cbDate = new() { Text = "Show date" };
    private readonly CheckBox cb24h = new() { Text = "Use 24-hour format" };
    private readonly CheckBox cbTop = new() { Text = "Always on top" };
    private readonly CheckBox cbTray = new() { Text = "Minimize to tray" };
    private readonly CheckBox cbStartMin = new() { Text = "Start minimized to tray" };

    private readonly CheckBox cbAlarmEnabled = new() { Text = "Enable alarm" };
    private readonly TextBox tbAlarm = new() { Width = 70 };
    private readonly NumericUpDown nudTimerMinutes = new() { Minimum = 1, Maximum = 24 * 60, Value = 5, Width = 70 };
    private readonly Button btnStartTimer = new() { Text = "Start timer" };
    private readonly Button btnStopTimer = new() { Text = "Stop timer" };
    private readonly Button btnOk = new() { Text = "OK", DialogResult = DialogResult.OK };
    private readonly Button btnCancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };
    private readonly Label lblTimerState = new() { AutoSize = true };
    private readonly Label lblAlarmHint = new() { AutoSize = true, Text = "Time (HH:mm)" };
    private readonly Label lblDigital = new() { AutoSize = true, Text = "Digital display:" };

    public SettingsForm()
    {
        Text = "Options";
        Font = new Font("Microsoft Sans Serif", 8.25f);
        BackColor = SystemColors.Control;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(370, 370);

        var tabs = new TabControl
        {
            Location = new Point(10, 10),
            Size = new Size(350, 295),
        };

        var tabDisplay = new TabPage("Display") { BackColor = SystemColors.Control };
        var tabAlarm = new TabPage("Alarm/Timer") { BackColor = SystemColors.Control };

        tabs.TabPages.Add(tabDisplay);
        tabs.TabPages.Add(tabAlarm);
        Controls.Add(tabs);

        lblDigital.Location = new Point(12, 18);
        cmbDigitalStyle.Location = new Point(115, 14);
        cmbDigitalStyle.Width = 200;
        cmbDigitalStyle.Items.AddRange(new object[] { "Off", "Terminal (green)", "Classic (grey)" });

        cbSeconds.Location = new Point(12, 52);
        cbRedSecond.Location = new Point(12, 78);
        cbSmoothSecond.Location = new Point(12, 104);
        cbDate.Location = new Point(12, 130);
        cb24h.Location = new Point(12, 156);
        cbTop.Location = new Point(12, 182);
        cbTray.Location = new Point(12, 208);
        cbStartMin.Location = new Point(12, 234);

        tabDisplay.Controls.Add(lblDigital);
        tabDisplay.Controls.Add(cmbDigitalStyle);
        tabDisplay.Controls.Add(cbSeconds);
        tabDisplay.Controls.Add(cbRedSecond);
        tabDisplay.Controls.Add(cbSmoothSecond);
        tabDisplay.Controls.Add(cbDate);
        tabDisplay.Controls.Add(cb24h);
        tabDisplay.Controls.Add(cbTop);
        tabDisplay.Controls.Add(cbTray);
        tabDisplay.Controls.Add(cbStartMin);

        cbAlarmEnabled.Location = new Point(12, 14);
        lblAlarmHint.Location = new Point(12, 42);
        tbAlarm.Location = new Point(115, 38);

        var gbTimer = new GroupBox
        {
            Text = "Countdown timer",
            Location = new Point(12, 75),
            Size = new Size(310, 130),
        };

        var lblMin = new Label { AutoSize = true, Text = "Minutes:", Location = new Point(12, 28) };
        nudTimerMinutes.Location = new Point(75, 24);
        btnStartTimer.Location = new Point(12, 55);
        btnStopTimer.Location = new Point(115, 55);
        lblTimerState.Location = new Point(12, 92);

        gbTimer.Controls.Add(lblMin);
        gbTimer.Controls.Add(nudTimerMinutes);
        gbTimer.Controls.Add(btnStartTimer);
        gbTimer.Controls.Add(btnStopTimer);
        gbTimer.Controls.Add(lblTimerState);

        tabAlarm.Controls.Add(cbAlarmEnabled);
        tabAlarm.Controls.Add(lblAlarmHint);
        tabAlarm.Controls.Add(tbAlarm);
        tabAlarm.Controls.Add(gbTimer);

        btnOk.Location = new Point(205, 320);
        btnOk.Size = new Size(75, 26);
        btnCancel.Location = new Point(285, 320);
        btnCancel.Size = new Size(75, 26);

        Controls.Add(btnOk);
        Controls.Add(btnCancel);

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        LoadSettingsToUi();

        cmbDigitalStyle.SelectedIndexChanged += (_, __) => cb24h.Enabled = cmbDigitalStyle.SelectedIndex != 0;
        cbSeconds.CheckedChanged += (_, __) =>
        {
            cbRedSecond.Enabled = cbSeconds.Checked;
            cbSmoothSecond.Enabled = cbSeconds.Checked;
        };
        cbTray.CheckedChanged += (_, __) =>
        {
            cbStartMin.Enabled = cbTray.Checked;
            if (!cbTray.Checked)
            {
                cbStartMin.Checked = false;
            }
        };

        btnStartTimer.Click += (_, __) =>
        {
            AppSettings.Current.CountdownEndUtcTicks = (DateTime.UtcNow + TimeSpan.FromMinutes((double)nudTimerMinutes.Value)).Ticks;
            AppSettings.Save();
            UpdateTimerStateLabel();
        };

        btnStopTimer.Click += (_, __) =>
        {
            AppSettings.Current.CountdownEndUtcTicks = 0;
            AppSettings.Save();
            UpdateTimerStateLabel();
        };

        btnOk.Click += (_, __) => SaveSettings();

        _ui.Tick += (_, __) => UpdateTimerStateLabel();
        _ui.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ui.Stop();
            _ui.Dispose();
        }

        base.Dispose(disposing);
    }

    private void LoadSettingsToUi()
    {
        var s = AppSettings.Current;
        cmbDigitalStyle.SelectedIndex = (int)s.DigitalStyle;
        cbSeconds.Checked = s.ShowSeconds;
        cbRedSecond.Checked = s.RedSecondHand;
        cbSmoothSecond.Checked = s.SmoothSecondHand;
        cbDate.Checked = s.ShowDate;
        cb24h.Checked = s.Use24Hour;
        cbTop.Checked = s.AlwaysOnTop;
        cbTray.Checked = s.MinimizeToTray;
        cbStartMin.Checked = s.StartMinimizedToTray;
        cbAlarmEnabled.Checked = s.AlarmEnabled;
        tbAlarm.Text = s.AlarmTimeHHmm;

        cb24h.Enabled = cmbDigitalStyle.SelectedIndex != 0;
        cbRedSecond.Enabled = cbSeconds.Checked;
        cbSmoothSecond.Enabled = cbSeconds.Checked;
        cbStartMin.Enabled = cbTray.Checked;

        UpdateTimerStateLabel();
    }

    private void UpdateTimerStateLabel()
    {
        long endTicks = AppSettings.Current.CountdownEndUtcTicks;
        if (endTicks <= 0)
        {
            lblTimerState.Text = "Status: stopped";
            btnStopTimer.Enabled = false;
            return;
        }

        var end = new DateTime(endTicks, DateTimeKind.Utc);
        var remaining = end - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            lblTimerState.Text = "Status: finishing...";
            btnStopTimer.Enabled = true;
            return;
        }

        btnStopTimer.Enabled = true;
        lblTimerState.Text = $"Status: running ({(int)remaining.TotalMinutes:D2}:{remaining.Seconds:D2} remaining)";
    }

    private void SaveSettings()
    {
        string alarm = tbAlarm.Text.Trim();
        if (!TimeSpan.TryParse(alarm, out _))
        {
            MessageBox.Show(this,
                "Please enter alarm time as HH:mm (e.g. 07:30).",
                "Invalid time",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        var s = AppSettings.Current;
        s.DigitalStyle = (DigitalStyle)cmbDigitalStyle.SelectedIndex;
        s.ShowSeconds = cbSeconds.Checked;
        s.RedSecondHand = cbRedSecond.Checked;
        s.SmoothSecondHand = cbSmoothSecond.Checked;
        s.ShowDate = cbDate.Checked;
        s.Use24Hour = cb24h.Checked;
        s.AlwaysOnTop = cbTop.Checked;
        s.MinimizeToTray = cbTray.Checked;
        s.StartMinimizedToTray = cbStartMin.Checked;
        s.AlarmEnabled = cbAlarmEnabled.Checked;
        s.AlarmTimeHHmm = alarm;

        AppSettings.Save();
    }
}
