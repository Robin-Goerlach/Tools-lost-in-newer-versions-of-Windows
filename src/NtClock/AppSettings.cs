using System;
using System.IO;
using System.Text.Json;

namespace NtClock;

public enum DigitalStyle
{
    Off = 0,
    Terminal = 1,
    Classic = 2,
}

internal sealed class AppSettings
{
    public bool ShowSeconds { get; set; } = true;
    public bool RedSecondHand { get; set; } = true;
    public bool SmoothSecondHand { get; set; } = false;
    public bool ShowDate { get; set; } = true;
    public bool Use24Hour { get; set; } = true;
    public bool AlwaysOnTop { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool StartMinimizedToTray { get; set; } = false;
    public DigitalStyle DigitalStyle { get; set; } = DigitalStyle.Terminal;
    public bool AlarmEnabled { get; set; } = false;
    public string AlarmTimeHHmm { get; set; } = "07:30";
    public string AlarmLastFiredStamp { get; set; } = string.Empty;
    public long CountdownEndUtcTicks { get; set; } = 0;
    public int WinX { get; set; } = -1;
    public int WinY { get; set; } = -1;
    public int WinW { get; set; } = -1;
    public int WinH { get; set; } = -1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "NtClock");

    private static readonly string SettingsPath = Path.Combine(SettingsDirectory, "settings.json");

    public static AppSettings Current { get; private set; } = Load();

    private static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new AppSettings();
            }

            string json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            string json = JsonSerializer.Serialize(Current, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // keep app usable even if settings cannot be written
        }
    }
}
