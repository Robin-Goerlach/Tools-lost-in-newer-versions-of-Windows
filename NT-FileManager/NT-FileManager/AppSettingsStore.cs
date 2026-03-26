using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace RetroNtFileManager
{
    /// <summary>
    /// Wurzelobjekt für anwendungsweite Einstellungen.
    /// </summary>
    [Serializable]
    public sealed class AppSettings
    {
        /// <summary>
        /// Gespeichertes Layout des Hauptfensters.
        /// </summary>
        public WindowLayoutSettings MainWindow { get; set; }
    }

    /// <summary>
    /// Serialisierbares Datenmodell für die Lage und Größe eines Fensters.
    /// </summary>
    [Serializable]
    public sealed class WindowLayoutSettings
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public FormWindowState WindowState { get; set; }

        /// <summary>
        /// Wandelt die gespeicherten Koordinaten in ein Rectangle um.
        /// </summary>
        public Rectangle ToRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        /// <summary>
        /// Verhindert die Wiederherstellung eines minimierten Zustands beim nächsten Start.
        /// </summary>
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

    /// <summary>
    /// Lädt und speichert die Anwendungseinstellungen als XML-Datei im LocalAppData-Bereich.
    /// </summary>
    internal static class AppSettingsStore
    {
        /// <summary>
        /// Zentraler Serializer für die AppSettings-Datei.
        /// </summary>
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(AppSettings));

        /// <summary>
        /// Lädt die Einstellungen von Datenträger oder liefert ein leeres Standardobjekt.
        /// </summary>
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
                // Die Anwendung soll auch dann starten können, wenn die Settings-Datei beschädigt ist.
                return new AppSettings();
            }
        }

        /// <summary>
        /// Speichert die Einstellungen auf Datenträger.
        /// </summary>
        /// <param name="settings">Zu speicherndes Einstellungsobjekt.</param>
        /// <summary>
        /// Speichert die Einstellungen auf Datenträger.
        /// </summary>
        /// <param name="settings">Zu speicherndes Einstellungsobjekt.</param>
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
                // Fehler beim Speichern dürfen den Programmabbruch nicht blockieren.
            }
        }

        /// <summary>
        /// Ermittelt den festen Ablageort der XML-Einstellungsdatei.
        /// </summary>
        private static string GetSettingsFilePath()
        {
            string baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(baseDirectory, "SASD", "FileManager", "settings.xml");
        }
    }

    /// <summary>
    /// Kapselt Hilfslogik für sichere Fensterpositionen auf wechselnden Monitor-Setups.
    /// </summary>
    internal static class WindowLayoutHelper
    {
        /// <summary>
        /// Liefert sichere Fenstergrenzen für den aktuellen Monitorverbund.
        /// </summary>
        public static Rectangle GetSafeBounds(Rectangle requestedBounds, Size minimumSize, Size fallbackSize)
        {
            Rectangle candidate = NormalizeBounds(requestedBounds, minimumSize, fallbackSize);

            if (IsVisibleOnAnyScreen(candidate))
            {
                return candidate;
            }

            // Fallback auf eine zentrierte, sichtbare Position, falls das gespeicherte Layout off-screen liegt.
            return GetCenteredPrimaryBounds(fallbackSize, minimumSize);
        }

        /// <summary>
        /// Berechnet eine zentrierte Standardposition auf dem primären Bildschirm.
        /// </summary>
        public static Rectangle GetCenteredPrimaryBounds(Size desiredSize, Size minimumSize)
        {
            Rectangle workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1600, 900);
            int width = Math.Max(minimumSize.Width, Math.Min(desiredSize.Width, workingArea.Width));
            int height = Math.Max(minimumSize.Height, Math.Min(desiredSize.Height, workingArea.Height));
            int x = workingArea.Left + Math.Max(0, (workingArea.Width - width) / 2);
            int y = workingArea.Top + Math.Max(0, (workingArea.Height - height) / 2);

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Stellt sicher, dass das gespeicherte Rechteck sinnvolle Mindestwerte besitzt.
        /// </summary>
        private static Rectangle NormalizeBounds(Rectangle bounds, Size minimumSize, Size fallbackSize)
        {
            int width = bounds.Width > 0 ? bounds.Width : fallbackSize.Width;
            int height = bounds.Height > 0 ? bounds.Height : fallbackSize.Height;

            width = Math.Max(width, minimumSize.Width);
            height = Math.Max(height, minimumSize.Height);

            return new Rectangle(bounds.X, bounds.Y, width, height);
        }

        /// <summary>
        /// Prüft, ob sich das angegebene Fensterrechteck zumindest teilweise auf einem sichtbaren Bildschirm befindet.
        /// </summary>
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
