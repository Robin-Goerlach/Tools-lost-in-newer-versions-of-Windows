using System;
using System.IO;

namespace RetroNtFileManager
{
    /// <summary>
    /// Normalisiert Pfade für die Zwischenablage in Windows- oder Linux-/WSL-Darstellung.
    /// </summary>
    internal static class PathClipboardHelper
    {
        /// <summary>
        /// Gibt einen vollständigen Windows-Pfad zurück.
        /// </summary>
        public static string ToWindowsPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return Path.GetFullPath(path);
        }

        /// <summary>
        /// Wandelt einen Windows-Pfad in eine WSL-kompatible /mnt/...-Darstellung um.
        /// </summary>
        public static string ToLinuxPath(string path)
        {
            string windowsPath = ToWindowsPath(path);
            if (string.IsNullOrWhiteSpace(windowsPath))
            {
                return string.Empty;
            }

            string root = Path.GetPathRoot(windowsPath);
            if (string.IsNullOrWhiteSpace(root) || root.Length < 2 || root[1] != ':')
            {
                // Nicht-lokale Pfade werden zumindest slash-normalisiert zurückgegeben.
                return windowsPath.Replace('\\', '/');
            }

            char driveLetter = char.ToLowerInvariant(root[0]);
            string relative = windowsPath.Substring(root.Length).Replace('\\', '/');
            return "/mnt/" + driveLetter + (string.IsNullOrEmpty(relative) ? string.Empty : "/" + relative.TrimStart('/'));
        }
    }
}
