using System.Drawing;
using System.Reflection;

namespace RetroNtFileManager
{
    internal static class ProductInfo
    {
        public const string MainWindowTitle = "SASD - Filemanager";
        public const string ProductDisplayName = "SASD - Filemanager";        
        public const string WebsiteUrl = "http://www.sasd.de"; // TODO: auf https umstellen
        public const string RepositoryUrl = "https://github.com/Robin-Goerlach/Tools-lost-in-newer-versions-of-Windows/tree/main/NT-FileManager";
        public static readonly Size DefaultMainWindowSize = new Size(1200, 800);

        public static string GetDisplayVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyInformationalVersionAttribute informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (informationalVersion != null && !string.IsNullOrWhiteSpace(informationalVersion.InformationalVersion))
            {
                return informationalVersion.InformationalVersion;
            }

            return assembly.GetName().Version?.ToString() ?? "0.0.0";
        }
    }
}
