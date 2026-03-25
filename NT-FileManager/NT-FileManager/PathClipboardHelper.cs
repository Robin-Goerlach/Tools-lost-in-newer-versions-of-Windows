using System;
using System.IO;

namespace RetroNtFileManager
{
    internal static class PathClipboardHelper
    {
        public static string ToWindowsPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return Path.GetFullPath(path);
        }

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
                return windowsPath.Replace('\\', '/');
            }

            char driveLetter = char.ToLowerInvariant(root[0]);
            string relative = windowsPath.Substring(root.Length).Replace('\\', '/');
            return "/mnt/" + driveLetter + (string.IsNullOrEmpty(relative) ? string.Empty : "/" + relative.TrimStart('/'));
        }
    }
}
