using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RetroNtFileManager
{
    internal sealed class PropertiesDialog : Form
    {
        public PropertiesDialog(string path)
        {
            Text = "Eigenschaften";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(420, 250);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(12)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            PictureBox iconBox = new PictureBox
            {
                Width = 32,
                Height = 32,
                SizeMode = PictureBoxSizeMode.CenterImage,
                Image = (Directory.Exists(path)
                    ? ShellIconHelper.GetLargeIcon(path, true)
                    : ShellIconHelper.GetLargeIcon(path, false)).ToBitmap()
            };

            Panel headerPanel = new Panel { Dock = DockStyle.Fill, Height = 42 };
            Label pathLabel = new Label
            {
                AutoSize = true,
                Left = 42,
                Top = 8,
                Text = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar))
            };
            Label fullPathLabel = new Label
            {
                AutoEllipsis = true,
                Left = 42,
                Top = 24,
                Width = 330,
                Text = path
            };
            headerPanel.Controls.Add(iconBox);
            headerPanel.Controls.Add(pathLabel);
            headerPanel.Controls.Add(fullPathLabel);
            iconBox.Left = 0;
            iconBox.Top = 2;

            layout.Controls.Add(headerPanel, 0, 0);
            layout.SetColumnSpan(headerPanel, 2);

            AddRow(layout, 1, "Typ:", GetTypeDescription(path));
            AddRow(layout, 2, "Ort:", GetLocation(path));
            AddRow(layout, 3, "Größe:", GetSizeDescription(path));
            AddRow(layout, 4, "Erstellt:", GetCreatedDescription(path));
            AddRow(layout, 5, "Geändert:", GetModifiedDescription(path));
            AddRow(layout, 6, "Attribute:", GetAttributesDescription(path));

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill
            };
            Button okButton = new Button
            {
                Text = "OK",
                Width = 75,
                DialogResult = DialogResult.OK
            };
            buttonPanel.Controls.Add(okButton);
            layout.Controls.Add(buttonPanel, 0, 7);
            layout.SetColumnSpan(buttonPanel, 2);

            AcceptButton = okButton;
            Controls.Add(layout);
            ClassicTheme.Apply(this);
        }

        private static void AddRow(TableLayoutPanel layout, int row, string label, string value)
        {
            Label left = new Label
            {
                AutoSize = true,
                Text = label,
                Margin = new Padding(0, 8, 0, 0)
            };

            TextBox right = new TextBox
            {
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                Text = value,
                Dock = DockStyle.Top
            };

            layout.Controls.Add(left, 0, row);
            layout.Controls.Add(right, 1, row);
        }

        private static string GetTypeDescription(string path)
        {
            if (Directory.Exists(path))
            {
                return "Dateiordner";
            }

            string extension = Path.GetExtension(path);
            return string.IsNullOrWhiteSpace(extension)
                ? "Datei"
                : extension.ToUpperInvariant() + "-Datei";
        }

        private static string GetLocation(string path)
        {
            return Path.GetDirectoryName(path) ?? path;
        }

        private static string GetSizeDescription(string path)
        {
            if (File.Exists(path))
            {
                long size = new FileInfo(path).Length;
                return $"{size:N0} Bytes";
            }

            if (Directory.Exists(path))
            {
                long total = 0;
                try
                {
                    foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            total += new FileInfo(file).Length;
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }

                return $"{total:N0} Bytes";
            }

            return string.Empty;
        }

        private static string GetCreatedDescription(string path)
        {
            return (File.Exists(path)
                    ? File.GetCreationTime(path)
                    : Directory.GetCreationTime(path))
                .ToString("dd.MM.yyyy HH:mm:ss");
        }

        private static string GetModifiedDescription(string path)
        {
            return (File.Exists(path)
                    ? File.GetLastWriteTime(path)
                    : Directory.GetLastWriteTime(path))
                .ToString("dd.MM.yyyy HH:mm:ss");
        }

        private static string GetAttributesDescription(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            return attributes.ToString();
        }
    }
}
