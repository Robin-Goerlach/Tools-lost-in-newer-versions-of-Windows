using System.Drawing;
using System.Reflection;

namespace RetroNtFileManager
{
    /// <summary>
    /// Stellt zentrale Produktmetadaten für Branding, Version und Standardwerte bereit.
    /// </summary>
    internal static class ProductInfo
    {
        /// <summary>
        /// Stabiler Produkttitel für die Haupttitelleiste.
        /// </summary>
        public const string MainWindowTitle = "SASD - Filemanager";

        /// <summary>
        /// Anzeigename des Produkts für Dialoge und Meldungen.
        /// </summary>
        public const string ProductDisplayName = "SASD - Filemanager";

        /// <summary>
        /// Öffentliche Zieladresse der SASD-Website.
        /// </summary>
        public const string WebsiteUrl = "https://www.sasd.de";

        /// <summary>
        /// Repository-Link für technische Nachverfolgbarkeit und Quellcodezugriff.
        /// </summary>
        public const string RepositoryUrl = "https://github.com/Robin-Goerlach/Tools-lost-in-newer-versions-of-Windows/tree/main/NT-FileManager";

        /// <summary>
        /// Fallback-Größe des Hauptfensters, falls keine gespeicherten Layoutdaten vorliegen.
        /// </summary>
        public static readonly Size DefaultMainWindowSize = new Size(1200, 800);

        /// <summary>
        /// Liest bevorzugt die InformationalVersion aus den Assembly-Metadaten.
        /// </summary>
        /// <returns>Benutzerfreundliche Versionszeichenfolge.</returns>
        /// <summary>
        /// Liest bevorzugt die InformationalVersion aus den Assembly-Metadaten.
        /// </summary>
        /// <returns>Benutzerfreundliche Versionszeichenfolge.</returns>
        public static string GetDisplayVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyInformationalVersionAttribute informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (informationalVersion != null && !string.IsNullOrWhiteSpace(informationalVersion.InformationalVersion))
            {
                return informationalVersion.InformationalVersion;
            }

            // Fallback auf die numerische Assembly-Version, falls keine informelle Version gesetzt ist.
            return assembly.GetName().Version?.ToString() ?? "0.0.0";
        }
    }
}
