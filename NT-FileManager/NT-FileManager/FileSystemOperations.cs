using System;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace RetroNtFileManager
{
    /// <summary>
    /// Kapselt Dateisystemoperationen für Dateien und Verzeichnisse.
    /// </summary>
    internal static class FileSystemOperations
    {
        /// <summary>
        /// Kopiert eine Datei oder ein Verzeichnis in ein Zielverzeichnis.
        /// </summary>
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

        /// <summary>
        /// Verschiebt eine Datei oder ein Verzeichnis in ein Zielverzeichnis.
        /// </summary>
        public static void MoveEntry(string sourcePath, string destinationDirectory)
        {
            if (File.Exists(sourcePath))
            {
                string targetPath = Path.Combine(destinationDirectory, Path.GetFileName(sourcePath));
                if (string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Bestehende Zieldateien werden im aktuellen Verhalten bewusst ersetzt.
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

        /// <summary>
        /// Löscht eine Datei oder ein Verzeichnis über den Papierkorb.
        /// </summary>
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

        /// <summary>
        /// Benennt eine Datei oder ein Verzeichnis um.
        /// </summary>
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

        /// <summary>
        /// Erstellt im Zielverzeichnis einen eindeutig benannten neuen Ordner.
        /// </summary>
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

        /// <summary>
        /// Kopiert ein Verzeichnis rekursiv einschließlich aller Unterordner und Dateien.
        /// </summary>
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
