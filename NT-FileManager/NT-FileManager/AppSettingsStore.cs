using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace RetroNtFileManager
{
    [Serializable]
    public sealed class AppSettings
    {
        public WindowLayoutSettings MainWindow { get; set; }
    }

    [Serializable]
    public sealed class WindowLayoutSettings
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public FormWindowState WindowState { get; set; }

        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        public FormWindowState GetSafeWindowState()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                return FormWindowState.Normal;
            }

            return WindowState == FormWindowState.Maximized
                ? FormWindowState.Maximized
                : FormWindowState.Normal;
        }
    }

    internal static class AppSettingsStore
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(AppSettings));

        public static AppSettings Load()
        {
            try
            {
                string settingsFile = GetSettingsFilePath();
                if (!File.Exists(settingsFile))
                {
                    return new AppSettings();
                }

                using (FileStream stream = File.OpenRead(settingsFile))
                {
                    return Serializer.Deserialize(stream) as AppSettings ?? new AppSettings();
                }
            }
            catch
            {
                // Die Anwendung soll auch dann starten können, wenn die Settings-Datei
                // beschädigt wurde oder vorübergehend nicht gelesen werden kann.
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            try
            {
                string settingsFile = GetSettingsFilePath();
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFile));

                using (FileStream stream = File.Create(settingsFile))
                {
                    Serializer.Serialize(stream, settings);
                }
            }
            catch
            {
                // Fehler beim Speichern sollen den Anwendungsschluss nicht blockieren.
                // Später kann hier Logging ergänzt werden.
            }
        }

        private static string GetSettingsFilePath()
        {
            string baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(baseDirectory, "SASD", "FileManager", "settings.xml");
        }
    }

    internal static class WindowLayoutHelper
    {
        public static Rectangle GetSafeBounds(Rectangle requestedBounds, Size minimumSize, Size fallbackSize)
        {
            Rectangle candidate = NormalizeBounds(requestedBounds, minimumSize, fallbackSize);

            if (IsVisibleOnAnyScreen(candidate))
            {
                return candidate;
            }

            return GetCenteredPrimaryBounds(fallbackSize, minimumSize);
        }

        public static Rectangle GetCenteredPrimaryBounds(Size desiredSize, Size minimumSize)
        {
            Rectangle workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1600, 900);
            int width = Math.Max(minimumSize.Width, Math.Min(desiredSize.Width, workingArea.Width));
            int height = Math.Max(minimumSize.Height, Math.Min(desiredSize.Height, workingArea.Height));
            int x = workingArea.Left + Math.Max(0, (workingArea.Width - width) / 2);
            int y = workingArea.Top + Math.Max(0, (workingArea.Height - height) / 2);

            return new Rectangle(x, y, width, height);
        }

        private static Rectangle NormalizeBounds(Rectangle bounds, Size minimumSize, Size fallbackSize)
        {
            int width = bounds.Width > 0 ? bounds.Width : fallbackSize.Width;
            int height = bounds.Height > 0 ? bounds.Height : fallbackSize.Height;

            width = Math.Max(width, minimumSize.Width);
            height = Math.Max(height, minimumSize.Height);

            return new Rectangle(bounds.X, bounds.Y, width, height);
        }

        private static bool IsVisibleOnAnyScreen(Rectangle bounds)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                Rectangle workingArea = screen.WorkingArea;
                if (workingArea.IntersectsWith(bounds))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
