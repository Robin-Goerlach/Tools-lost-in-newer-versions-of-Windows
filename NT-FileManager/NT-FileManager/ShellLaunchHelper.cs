using System;
using System.Diagnostics;
using System.IO;

namespace RetroNtFileManager
{
    internal static class ShellLaunchHelper
    {
        public static void OpenCommandPrompt(string workingDirectory)
        {
            string directory = NormalizeExistingDirectory(workingDirectory);

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/K cd /d \"" + directory + "\"",
                WorkingDirectory = directory,
                UseShellExecute = true
            });
        }

        private static string NormalizeExistingDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            string fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }

            string parent = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(parent) && Directory.Exists(parent))
            {
                return parent;
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }
}
