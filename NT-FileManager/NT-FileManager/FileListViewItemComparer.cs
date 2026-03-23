using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

namespace RetroNtFileManager
{
    internal sealed class FileListViewItemComparer : IComparer
    {
        public int ColumnIndex { get; set; }
        public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

        public int Compare(object x, object y)
        {
            if (!(x is ListViewItem left) || !(y is ListViewItem right))
            {
                return 0;
            }

            bool leftIsDir = IsDirectory(left);
            bool rightIsDir = IsDirectory(right);

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

        private static bool IsDirectory(ListViewItem item)
        {
            return string.Equals(GetTagValue(item, "Kind"), "Directory", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetTagValue(ListViewItem item, string key)
        {
            if (item.Tag is FileEntryInfo info)
            {
                return info.GetValue(key);
            }

            return string.Empty;
        }

        private static int CompareLong(string a, string b)
        {
            long.TryParse(a, out long left);
            long.TryParse(b, out long right);
            return left.CompareTo(right);
        }

        private static int CompareDate(string a, string b)
        {
            DateTime.TryParse(a, out DateTime left);
            DateTime.TryParse(b, out DateTime right);
            return left.CompareTo(right);
        }
    }

    internal sealed class FileEntryInfo
    {
        public string FullPath { get; set; }
        public string Kind { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public string Attributes { get; set; }

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
