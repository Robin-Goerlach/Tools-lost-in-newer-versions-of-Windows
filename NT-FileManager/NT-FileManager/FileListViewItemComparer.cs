using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

namespace RetroNtFileManager
{
    /// <summary>
    /// Vergleichslogik für die Dateiliste mit Verzeichnisvorrang und spaltenspezifischer Sortierung.
    /// </summary>
    internal sealed class FileListViewItemComparer : IComparer
    {
        /// <summary>
        /// Aktuell sortierte Spalte.
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// Aktuelle Sortierreihenfolge.
        /// </summary>
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

        /// <summary>
        /// Vergleicht zwei ListView-Einträge anhand der konfigurierten Spalte.
        /// </summary>
        public int Compare(object x, object y)
        {
            if (!(x is ListViewItem left) || !(y is ListViewItem right))
            {
                return 0;
            }

            bool leftIsDir = IsDirectory(left);
            bool rightIsDir = IsDirectory(right);

            // Verzeichnisse sollen immer vor Dateien erscheinen.
            if (leftIsDir != rightIsDir)
            {
                return leftIsDir ? -1 : 1;
            }

            int result;
            switch (ColumnIndex)
            {
                case 2:
                    result = CompareLong(GetTagValue(left, "Size"), GetTagValue(right, "Size"));
                    break;

                case 4:
                    result = CompareDate(GetTagValue(left, "Modified"), GetTagValue(right, "Modified"));
                    break;

                default:
                    string leftText = ColumnIndex < left.SubItems.Count ? left.SubItems[ColumnIndex].Text : string.Empty;
                    string rightText = ColumnIndex < right.SubItems.Count ? right.SubItems[ColumnIndex].Text : string.Empty;
                    result = string.Compare(leftText, rightText, true, CultureInfo.CurrentCulture);
                    break;
            }

            return SortOrder == SortOrder.Descending ? -result : result;
        }

        /// <summary>
        /// Prüft, ob ein Eintrag einen Verzeichnisdatensatz repräsentiert.
        /// </summary>
        private static bool IsDirectory(ListViewItem item)
        {
            return string.Equals(GetTagValue(item, "Kind"), "Directory", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Liest einen Wert aus dem hinterlegten Dateimetadatenobjekt.
        /// </summary>
        private static string GetTagValue(ListViewItem item, string key)
        {
            if (item.Tag is FileEntryInfo info)
            {
                return info.GetValue(key);
            }

            return string.Empty;
        }

        /// <summary>
        /// Vergleicht zwei Ganzzahlen im Stringformat.
        /// </summary>
        private static int CompareLong(string a, string b)
        {
            long.TryParse(a, out long left);
            long.TryParse(b, out long right);
            return left.CompareTo(right);
        }

        /// <summary>
        /// Vergleicht zwei Datumswerte im Stringformat.
        /// </summary>
        private static int CompareDate(string a, string b)
        {
            DateTime.TryParse(a, out DateTime left);
            DateTime.TryParse(b, out DateTime right);
            return left.CompareTo(right);
        }
    }

    /// <summary>
    /// Metadatenobjekt, das ListView-Einträgen für Sortierung und Folgeoperationen angehängt wird.
    /// </summary>
    internal sealed class FileEntryInfo
    {
        public string FullPath { get; set; }
        public string Kind { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public string Attributes { get; set; }

        /// <summary>
        /// Gibt einen Wert anhand eines logischen Schlüssels zurück.
        /// </summary>
        public string GetValue(string key)
        {
            switch (key)
            {
                case "Path":
                    return FullPath ?? string.Empty;
                case "Kind":
                    return Kind ?? string.Empty;
                case "Size":
                    return Size.ToString(CultureInfo.InvariantCulture);
                case "Modified":
                    return Modified.ToString("O", CultureInfo.InvariantCulture);
                case "Attributes":
                    return Attributes ?? string.Empty;
                default:
                    return string.Empty;
            }
        }
    }
}
