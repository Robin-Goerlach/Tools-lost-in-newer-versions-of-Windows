using System;
using System.IO;
using System.Text.Json;

namespace NtClock
{
    internal sealed class NtSettings
    {
        public bool ShowTitleBar { get; set; } = true;
        public bool AlwaysOnTop { get; set; } = false;
        public bool ShowSeconds { get; set; } = true;
        public bool ShowDigitalTime { get; set; } = false;
        public bool ShowDate { get; set; } = false;
        public bool Use24Hour { get; set; } = true;
        public bool HideToTrayOnClose { get; set; } = true;
        public bool StartMinimizedToTray { get; set; } = false;
        public bool SmoothSecondHand { get; set; } = false;
        public int Left { get; set; } = -1;
        public int Top { get; set; } = -1;
        public int AlarmHour { get; set; } = 7;
        public int AlarmMinute { get; set; } = 30;
        public bool AlarmEnabled { get; set; } = false;
        public string LastAlarmStamp { get; set; } = string.Empty;

        private static string SettingsDirectory
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(basePath, "NtClock");
            }
        }

        private static string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

        public static NtSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    return new NtSettings();
                }

                var json = File.ReadAllText(SettingsPath);
                var loaded = JsonSerializer.Deserialize<NtSettings>(json);
                return loaded ?? new NtSettings();
            }
            catch
            {
                return new NtSettings();
            }
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // ignore settings write errors in classic utility style
            }
        }
    }
}
