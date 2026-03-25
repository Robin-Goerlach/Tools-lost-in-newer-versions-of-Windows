using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace RetroNtFileManager
{
    internal sealed class AboutDialog : Form
    {
        public AboutDialog()
        {
            Text = "Über SASD - Filemanager";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(520, 300);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(14)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label titleLabel = new Label
            {
                AutoSize = true,
                Font = new Font(ClassicTheme.UiFont, FontStyle.Bold),
                Text = ProductInfo.ProductDisplayName
            };

            Label versionLabel = new Label
            {
                AutoSize = true,
                Text = "Version: " + ProductInfo.GetDisplayVersion()
            };

            Label descriptionLabel = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(480, 0),
                Text = "Retro-inspirierter Datei-Manager auf Basis von C# und Windows Forms. " +
                       "Der 1.1-Grundausbau führt SASD-Branding, Versionsanzeige sowie die " +
                       "technische Basis für persistente Einstellungen und Fensterwiederherstellung ein."
            };

            FlowLayoutPanel linksPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Margin = new Padding(0, 8, 0, 0)
            };

            LinkLabel websiteLink = CreateLink("Website", ProductInfo.WebsiteUrl);
            LinkLabel repositoryLink = CreateLink("Repository", ProductInfo.RepositoryUrl);
            linksPanel.Controls.Add(websiteLink);
            linksPanel.Controls.Add(new Label { AutoSize = true, Text = "|", Padding = new Padding(6, 3, 6, 0) });
            linksPanel.Controls.Add(repositoryLink);

            TextBox notesBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Text = "Diese Fassung ist bewusst noch kein vollständiges 1.1-Release.\r\n\r\n" +
                       "Enthalten sind:\r\n" +
                       "- SASD-Produktbezeichnung in der Haupttitelzeile\r\n" +
                       "- About-Dialog mit Versionsnummer und Links\r\n" +
                       "- technischer Settings-Speicher unter %LocalAppData%\\SASD\\FileManager\r\n" +
                       "- Wiederherstellung von Fenstergröße, Position und Zustand\r\n" +
                       "- Off-screen-Rettung für Mehrmonitor-Szenarien\r\n"
            };

            Button closeButton = new Button
            {
                Text = "Schließen",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Right,
                AutoSize = true
            };

            AcceptButton = closeButton;
            CancelButton = closeButton;

            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(versionLabel, 0, 1);
            layout.Controls.Add(descriptionLabel, 0, 2);
            layout.Controls.Add(linksPanel, 0, 3);
            layout.Controls.Add(notesBox, 0, 4);
            layout.Controls.Add(closeButton, 0, 5);

            Controls.Add(layout);
            ClassicTheme.Apply(this);
        }

        private static LinkLabel CreateLink(string caption, string targetUrl)
        {
            LinkLabel link = new LinkLabel
            {
                AutoSize = true,
                Text = caption,
                Tag = targetUrl
            };

            link.LinkClicked += (_, __) => OpenUrl(targetUrl);
            return link;
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Der Link konnte nicht geöffnet werden.\r\n\r\n" + ex.Message,
                    "Link öffnen",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
