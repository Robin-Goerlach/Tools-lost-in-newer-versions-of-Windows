using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace RetroNtFileManager
{
    /// <summary>
    /// Liest Dateisymbole über die Windows-Shell und cached sie für bessere Performance.
    /// </summary>
    internal static class ShellIconHelper
    {
        private const uint ShgfiIcon = 0x000000100;
        private const uint ShgfiSmallIcon = 0x000000001;
        private const uint ShgfiLargeIcon = 0x000000000;
        private const uint ShgfiUseFileAttributes = 0x000000010;
        private const uint ShgfiOpenIcon = 0x000000002;
        private const uint FileAttributeDirectory = 0x00000010;
        private const uint FileAttributeNormal = 0x00000080;

        /// <summary>
        /// Cache für kleine Icons je Typ/Erweiterung.
        /// </summary>
        private static readonly Dictionary<string, Icon> SmallIconCache = new Dictionary<string, Icon>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Cache für große Icons je Typ/Erweiterung.
        /// </summary>
        private static readonly Dictionary<string, Icon> LargeIconCache = new Dictionary<string, Icon>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Liefert ein kleines Symbol für Datei oder Verzeichnis.
        /// </summary>
        public static Icon GetSmallIcon(string path, bool isDirectory)
        {
            return GetCachedIcon(SmallIconCache, path, isDirectory, true);
        }

        /// <summary>
        /// Liefert ein großes Symbol für Datei oder Verzeichnis.
        /// </summary>
        public static Icon GetLargeIcon(string path, bool isDirectory)
        {
            return GetCachedIcon(LargeIconCache, path, isDirectory, false);
        }

        /// <summary>
        /// Holt ein Icon aus dem Cache oder liest es einmalig über die Shell.
        /// </summary>
        private static Icon GetCachedIcon(Dictionary<string, Icon> cache, string path, bool isDirectory, bool small)
        {
            string key = BuildCacheKey(path, isDirectory);
            if (cache.TryGetValue(key, out Icon cached))
            {
                return cached;
            }

            Icon icon = GetShellIcon(path, isDirectory, small) ?? SystemIcons.WinLogo;
            cache[key] = icon;
            return icon;
        }

        /// <summary>
        /// Ermittelt den Cache-Schlüssel für Datei oder Verzeichnis.
        /// </summary>
        private static string BuildCacheKey(string path, bool isDirectory)
        {
            if (isDirectory)
            {
                return "folder";
            }

            string extension = Path.GetExtension(path);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return "file";
            }

            return extension;
        }

        /// <summary>
        /// Liest das Dateisymbol direkt über SHGetFileInfo.
        /// </summary>
        private static Icon GetShellIcon(string path, bool isDirectory, bool small)
        {
            uint attributes = isDirectory ? FileAttributeDirectory : FileAttributeNormal;
            uint flags = ShgfiIcon | ShgfiUseFileAttributes | (small ? ShgfiSmallIcon : ShgfiLargeIcon);

            if (isDirectory)
            {
                flags |= ShgfiOpenIcon;
            }

            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr result = SHGetFileInfo(path, attributes, ref shinfo, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), flags);
            if (result == IntPtr.Zero || shinfo.hIcon == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                using (Icon temporary = Icon.FromHandle(shinfo.hIcon))
                {
                    return (Icon)temporary.Clone();
                }
            }
            finally
            {
                DestroyIcon(shinfo.hIcon);
            }
        }

        /// <summary>
        /// Shell-API zum Lesen der Dateiinformationen.
        /// </summary>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        /// <summary>
        /// Shell-API zum Lesen der Dateiinformationen.
        /// </summary>
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        /// <summary>
        /// Gibt ein von der Shell geliefertes Icon-Handle wieder frei.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        /// <summary>
        /// Gibt ein von der Shell geliefertes Icon-Handle wieder frei.
        /// </summary>
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// Native Datenstruktur für SHGetFileInfo.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
    }
}
