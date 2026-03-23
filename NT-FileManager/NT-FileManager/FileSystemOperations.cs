using System;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace RetroNtFileManager
{
    internal static class FileSystemOperations
    {
        public static void CopyEntry(string sourcePath, string destinationDirectory)
        {
            if (File.Exists(sourcePath))
            {
                string targetPath = Path.Combine(destinationDirectory, Path.GetFileName(sourcePath));
                File.Copy(sourcePath, targetPath, overwrite: true);
                return;
            }

            if (Directory.Exists(sourcePath))
            {
                string targetPath = Path.Combine(destinationDirectory, new DirectoryInfo(sourcePath).Name);
                CopyDirectory(sourcePath, targetPath);
                return;
            }

            throw new FileNotFoundException("Quelle wurde nicht gefunden.", sourcePath);
        }

        public static void MoveEntry(string sourcePath, string destinationDirectory)
        {
            if (File.Exists(sourcePath))
            {
                string targetPath = Path.Combine(destinationDirectory, Path.GetFileName(sourcePath));
                if (string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                File.Move(sourcePath, targetPath);
                return;
            }

            if (Directory.Exists(sourcePath))
            {
                string targetPath = Path.Combine(destinationDirectory, new DirectoryInfo(sourcePath).Name);
                if (string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, recursive: true);
                }

                Directory.Move(sourcePath, targetPath);
                return;
            }

            throw new FileNotFoundException("Quelle wurde nicht gefunden.", sourcePath);
        }

        public static void DeleteEntry(string path)
        {
            if (File.Exists(path))
            {
                FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                return;
            }

            if (Directory.Exists(path))
            {
                FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                return;
            }

            throw new FileNotFoundException("Objekt wurde nicht gefunden.", path);
        }

        public static void RenameEntry(string sourcePath, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Der neue Name darf nicht leer sein.", nameof(newName));
            }

            string parentDirectory = Path.GetDirectoryName(sourcePath);
            string destination = Path.Combine(parentDirectory ?? string.Empty, newName);

            if (File.Exists(sourcePath))
            {
                File.Move(sourcePath, destination);
                return;
            }

            if (Directory.Exists(sourcePath))
            {
                Directory.Move(sourcePath, destination);
                return;
            }

            throw new FileNotFoundException("Objekt wurde nicht gefunden.", sourcePath);
        }

        public static string CreateNewFolder(string parentDirectory)
        {
            const string baseName = "Neuer Ordner";
            string candidate = Path.Combine(parentDirectory, baseName);
            int counter = 2;

            while (Directory.Exists(candidate) || File.Exists(candidate))
            {
                candidate = Path.Combine(parentDirectory, $"{baseName} ({counter})");
                counter++;
            }

            Directory.CreateDirectory(candidate);
            return candidate;
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string targetFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFile, overwrite: true);
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
                CopyDirectory(directory, targetSubDir);
            }
        }
    }
}
