using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RetroNtFileManager
{
    internal sealed class MainForm : Form
    {
        private readonly MenuStrip _menuStrip;
        private readonly ToolStrip _toolStrip;
        private readonly StatusStrip _statusStrip;
        private readonly ToolStripStatusLabel _statusLabel;
        private readonly AppSettings _appSettings;

        public MainForm()
        {
            _appSettings = AppSettingsStore.Load();

            Text = ProductInfo.MainWindowTitle;
            IsMdiContainer = true;
            StartPosition = FormStartPosition.Manual;
            MinimumSize = new Size(900, 600);
            BackColor = Color.Teal;
            KeyPreview = true;

            _menuStrip = BuildMenuStrip();
            _toolStrip = BuildToolStrip();

            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Bereit");
            _statusStrip.Items.Add(_statusLabel);

            Controls.Add(_statusStrip);
            Controls.Add(_toolStrip);
            Controls.Add(_menuStrip);
            MainMenuStrip = _menuStrip;

            Load += OnMainFormLoad;
            FormClosing += OnMainFormClosing;
            MdiChildActivate += (_, __) => UpdateWindowTitle();

            ClassicTheme.Apply(this);
        }

        private void OnMainFormLoad(object sender, EventArgs e)
        {
            RestoreWindowLayout();
            OpenNewWindow(GetStartupPath());
        }

        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowLayout();
        }

        private MenuStrip BuildMenuStrip()
        {
            MenuStrip menuStrip = new MenuStrip();

            ToolStripMenuItem fileMenu = new ToolStripMenuItem("&Datei");
            fileMenu.DropDownItems.Add(CreateItem("&Neues Fenster", "Ctrl+N", (_, __) => OpenNewWindow(GetCurrentDirectory())));
            fileMenu.DropDownItems.Add(CreateItem("&Aktives Fenster duplizieren", "Ctrl+Shift+D", (_, __) => OpenNewWindow(GetCurrentDirectory())));
            fileMenu.DropDownItems.Add(CreateItem("&Öffnen", "Enter", (_, __) => ActiveWindow?.OpenSelectedEntry()));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(CreateItem("&Kopieren...", "F5", (_, __) => ActiveWindow?.CopySelectedEntry()));
            fileMenu.DropDownItems.Add(CreateItem("Verschie&ben...", "F6", (_, __) => ActiveWindow?.MoveSelectedEntry()));
            fileMenu.DropDownItems.Add(CreateItem("&Umbenennen", "F2", (_, __) => ActiveWindow?.RenameSelectedEntry()));
            fileMenu.DropDownItems.Add(CreateItem("&Löschen", "Del", (_, __) => ActiveWindow?.DeleteSelectedEntry()));
            fileMenu.DropDownItems.Add(CreateItem("Neuer &Ordner", "Ctrl+Shift+N", (_, __) => ActiveWindow?.CreateFolder()));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(CreateItem("E&igenschaften", "Alt+Enter", (_, __) => ActiveWindow?.ShowPropertiesForSelection()));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(CreateItem("Be&enden", "Alt+F4", (_, __) => Close()));

            ToolStripMenuItem diskMenu = new ToolStripMenuItem("&Datenträger");
            diskMenu.DropDownItems.Add(CreateItem("Nach oben", "Backspace", (_, __) => ActiveWindow?.GoUpOneLevel()));
            diskMenu.DropDownItems.Add(CreateItem("A&ktualisieren", "Ctrl+R", (_, __) => ActiveWindow?.RefreshView()));
            diskMenu.DropDownItems.Add(new ToolStripSeparator());
            diskMenu.DropDownItems.Add(CreateItem("Pfad als &Windows-Pfad kopieren", "Ctrl+Shift+C", (_, __) => ActiveWindow?.CopyCurrentPathAsWindowsPath()));
            diskMenu.DropDownItems.Add(CreateItem("Pfad als &Linux-/WSL-Pfad kopieren", "Ctrl+Shift+L", (_, __) => ActiveWindow?.CopyCurrentPathAsLinuxPath()));
            diskMenu.DropDownItems.Add(CreateItem("&CMD hier öffnen", "Ctrl+Shift+K", (_, __) => ActiveWindow?.OpenCommandPromptHere()));

            ToolStripMenuItem treeMenu = new ToolStripMenuItem("&Baum");
            treeMenu.DropDownItems.Add(CreateItem("Verzweigung &erweitern", "Right", (_, __) => ActiveWindow?.ExpandCurrentTreeNode()));
            treeMenu.DropDownItems.Add(CreateItem("Verzweigung &reduzieren", "Left", (_, __) => ActiveWindow?.CollapseCurrentTreeNode()));

            ToolStripMenuItem viewMenu = new ToolStripMenuItem("&Ansicht");
            viewMenu.DropDownItems.Add(CreateItem("&Große Symbole", "Ctrl+1", (_, __) => ActiveWindow?.SetView(View.LargeIcon)));
            viewMenu.DropDownItems.Add(CreateItem("&Kleine Symbole", "Ctrl+2", (_, __) => ActiveWindow?.SetView(View.SmallIcon)));
            viewMenu.DropDownItems.Add(CreateItem("&Liste", "Ctrl+3", (_, __) => ActiveWindow?.SetView(View.List)));
            viewMenu.DropDownItems.Add(CreateItem("&Details", "Ctrl+4", (_, __) => ActiveWindow?.SetView(View.Details)));
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(CreateItem("Aktualisieren", "Ctrl+R", (_, __) => ActiveWindow?.RefreshView()));

            ToolStripMenuItem optionsMenu = new ToolStripMenuItem("&Optionen");
            ToolStripMenuItem fullRowSelectItem = new ToolStripMenuItem("Ganze Zeile markieren")
            {
                Checked = true,
                CheckOnClick = true
            };
            fullRowSelectItem.CheckedChanged += (_, __) =>
            {
                if (ActiveWindow != null)
                {
                    ActiveWindow.FullRowSelect = fullRowSelectItem.Checked;
                }
            };
            optionsMenu.DropDownItems.Add(fullRowSelectItem);

            ToolStripMenuItem windowMenu = new ToolStripMenuItem("&Fenster");
            windowMenu.DropDownItems.Add(CreateItem("&Nächstes Fenster", "Ctrl+Tab", (_, __) => ActivateNextWindow()));
            windowMenu.DropDownItems.Add(CreateItem("&Vorheriges Fenster", "Ctrl+Shift+Tab", (_, __) => ActivatePreviousWindow()));
            windowMenu.DropDownItems.Add(CreateItem("Aktives Fenster &schließen", "Ctrl+W", (_, __) => CloseActiveWindow()));
            windowMenu.DropDownItems.Add(new ToolStripSeparator());
            windowMenu.DropDownItems.Add(CreateItem("&Überlappend", "Ctrl+Alt+C", (_, __) => ApplyMdiLayout(MdiLayout.Cascade, "Fenster überlappend angeordnet")));
            windowMenu.DropDownItems.Add(CreateItem("&Nebeneinander", "Ctrl+Alt+V", (_, __) => ApplyMdiLayout(MdiLayout.TileVertical, "Fenster nebeneinander angeordnet")));
            windowMenu.DropDownItems.Add(CreateItem("&Untereinander", "Ctrl+Alt+H", (_, __) => ApplyMdiLayout(MdiLayout.TileHorizontal, "Fenster untereinander angeordnet")));
            windowMenu.DropDownItems.Add(CreateItem("Symbole anordnen", null, (_, __) => ApplyMdiLayout(MdiLayout.ArrangeIcons, "Minimierte Fenster neu angeordnet")));

            ToolStripMenuItem helpMenu = new ToolStripMenuItem("&Hilfe");
            helpMenu.DropDownItems.Add(CreateItem("&Über SASD - Filemanager...", null, (_, __) => ShowAboutDialog()));

            menuStrip.Items.AddRange(new ToolStripItem[]
            {
                fileMenu,
                diskMenu,
                treeMenu,
                viewMenu,
                optionsMenu,
                windowMenu,
                helpMenu
            });

            return menuStrip;
        }

        private ToolStrip BuildToolStrip()
        {
            ToolStrip toolStrip = new ToolStrip();

            toolStrip.Items.Add(CreateButton("Neu", (_, __) => OpenNewWindow(GetCurrentDirectory()), "Neues Fenster im aktuellen Verzeichnis"));
            toolStrip.Items.Add(CreateButton("Öffnen", (_, __) => ActiveWindow?.OpenSelectedEntry(), "Ausgewählten Eintrag öffnen"));
            toolStrip.Items.Add(new ToolStripSeparator());

            toolStrip.Items.Add(CreateButton("Kopieren", (_, __) => ActiveWindow?.CopySelectedEntry(), "Ausgewählte Einträge kopieren"));
            toolStrip.Items.Add(CreateButton("Verschieben", (_, __) => ActiveWindow?.MoveSelectedEntry(), "Ausgewählte Einträge verschieben"));
            toolStrip.Items.Add(CreateButton("Löschen", (_, __) => ActiveWindow?.DeleteSelectedEntry(), "Ausgewählte Einträge löschen"));
            toolStrip.Items.Add(CreateButton("Ordner", (_, __) => ActiveWindow?.CreateFolder(), "Neuen Ordner anlegen"));
            toolStrip.Items.Add(new ToolStripSeparator());

            toolStrip.Items.Add(CreateButton("Nach oben", (_, __) => ActiveWindow?.GoUpOneLevel(), "Eine Ebene nach oben wechseln"));
            toolStrip.Items.Add(CreateButton("Aktualisieren", (_, __) => ActiveWindow?.RefreshView(), "Aktives Verzeichnis aktualisieren"));
            toolStrip.Items.Add(new ToolStripSeparator());

            toolStrip.Items.Add(CreateButton("Pfad", (_, __) => ActiveWindow?.CopyCurrentPathAsWindowsPath(), "Aktuellen Pfad im Windows-Format kopieren"));
            toolStrip.Items.Add(CreateButton("Linux", (_, __) => ActiveWindow?.CopyCurrentPathAsLinuxPath(), "Aktuellen Pfad im Linux-/WSL-Format kopieren"));
            toolStrip.Items.Add(CreateButton("CMD", (_, __) => ActiveWindow?.OpenCommandPromptHere(), "CMD im aktuellen Verzeichnis öffnen"));
            toolStrip.Items.Add(new ToolStripSeparator());

            toolStrip.Items.Add(CreateButton("Kaskade", (_, __) => ApplyMdiLayout(MdiLayout.Cascade, "Fenster überlappend angeordnet"), "Fenster überlappend anordnen"));
            toolStrip.Items.Add(CreateButton("Links/Rechts", (_, __) => ApplyMdiLayout(MdiLayout.TileVertical, "Fenster nebeneinander angeordnet"), "Fenster nebeneinander anordnen"));
            toolStrip.Items.Add(CreateButton("Oben/Unten", (_, __) => ApplyMdiLayout(MdiLayout.TileHorizontal, "Fenster untereinander angeordnet"), "Fenster untereinander anordnen"));

            return toolStrip;
        }

        private ToolStripMenuItem CreateItem(string text, string shortcutDisplayText, EventHandler handler)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text)
            {
                ShowShortcutKeys = !string.IsNullOrWhiteSpace(shortcutDisplayText),
                ShortcutKeyDisplayString = shortcutDisplayText ?? string.Empty
            };
            item.Click += handler;
            return item;
        }

        private ToolStripButton CreateButton(string text, EventHandler handler, string toolTipText = null)
        {
            ToolStripButton button = new ToolStripButton(text)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                AutoSize = true,
                ToolTipText = toolTipText ?? text
            };
            button.Click += handler;
            return button;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (HandleGlobalShortcut(keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool HandleGlobalShortcut(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N:
                    OpenNewWindow(GetCurrentDirectory());
                    return true;

                case Keys.Control | Keys.Shift | Keys.D:
                    OpenNewWindow(GetCurrentDirectory());
                    return true;

                case Keys.Enter:
                    ActiveWindow?.OpenSelectedEntry();
                    return true;

                case Keys.F5:
                    ActiveWindow?.CopySelectedEntry();
                    return true;

                case Keys.F6:
                    ActiveWindow?.MoveSelectedEntry();
                    return true;

                case Keys.F2:
                    ActiveWindow?.RenameSelectedEntry();
                    return true;

                case Keys.Delete:
                    ActiveWindow?.DeleteSelectedEntry();
                    return true;

                case Keys.Control | Keys.Shift | Keys.N:
                    ActiveWindow?.CreateFolder();
                    return true;

                case Keys.Alt | Keys.Enter:
                    ActiveWindow?.ShowPropertiesForSelection();
                    return true;

                case Keys.Alt | Keys.F4:
                    Close();
                    return true;

                case Keys.Back:
                    ActiveWindow?.GoUpOneLevel();
                    return true;

                case Keys.Control | Keys.R:
                    ActiveWindow?.RefreshView();
                    return true;

                case Keys.Control | Keys.Shift | Keys.C:
                    ActiveWindow?.CopyCurrentPathAsWindowsPath();
                    return true;

                case Keys.Control | Keys.Shift | Keys.L:
                    ActiveWindow?.CopyCurrentPathAsLinuxPath();
                    return true;

                case Keys.Control | Keys.Shift | Keys.K:
                    ActiveWindow?.OpenCommandPromptHere();
                    return true;

                case Keys.Right:
                    ActiveWindow?.ExpandCurrentTreeNode();
                    return true;

                case Keys.Left:
                    ActiveWindow?.CollapseCurrentTreeNode();
                    return true;

                case Keys.Control | Keys.D1:
                    ActiveWindow?.SetView(View.LargeIcon);
                    return true;

                case Keys.Control | Keys.D2:
                    ActiveWindow?.SetView(View.SmallIcon);
                    return true;

                case Keys.Control | Keys.D3:
                    ActiveWindow?.SetView(View.List);
                    return true;

                case Keys.Control | Keys.D4:
                    ActiveWindow?.SetView(View.Details);
                    return true;

                case Keys.Control | Keys.Tab:
                    ActivateNextWindow();
                    return true;

                case Keys.Control | Keys.Shift | Keys.Tab:
                    ActivatePreviousWindow();
                    return true;

                case Keys.Control | Keys.W:
                    CloseActiveWindow();
                    return true;

                case Keys.Control | Keys.Alt | Keys.C:
                    ApplyMdiLayout(MdiLayout.Cascade, "Fenster überlappend angeordnet");
                    return true;

                case Keys.Control | Keys.Alt | Keys.V:
                    ApplyMdiLayout(MdiLayout.TileVertical, "Fenster nebeneinander angeordnet");
                    return true;

                case Keys.Control | Keys.Alt | Keys.H:
                    ApplyMdiLayout(MdiLayout.TileHorizontal, "Fenster untereinander angeordnet");
                    return true;

                default:
                    return false;
            }
        }

        private void OpenNewWindow(string path)
        {
            DirectoryWindowForm child = new DirectoryWindowForm(path, SetStatus)
            {
                MdiParent = this
            };
            child.Show();
            child.Activate();
            SetStatus($"Neues Fenster geöffnet: {path}");
        }

        private void ShowAboutDialog()
        {
            using (AboutDialog dialog = new AboutDialog())
            {
                dialog.ShowDialog(this);
            }
        }

        private void ActivateNextWindow()
        {
            CycleWindows(forward: true);
        }

        private void ActivatePreviousWindow()
        {
            CycleWindows(forward: false);
        }

        private void CycleWindows(bool forward)
        {
            Form[] windows = MdiChildren;
            if (windows == null || windows.Length == 0)
            {
                return;
            }

            if (windows.Length == 1)
            {
                windows[0].Activate();
                return;
            }

            Form active = ActiveMdiChild;
            int currentIndex = Array.IndexOf(windows, active);
            if (currentIndex < 0)
            {
                windows[0].Activate();
                return;
            }

            int nextIndex = forward
                ? (currentIndex + 1) % windows.Length
                : (currentIndex - 1 + windows.Length) % windows.Length;

            windows[nextIndex].Activate();
            SetStatus(forward ? "Zum nächsten Fenster gewechselt" : "Zum vorherigen Fenster gewechselt");
        }

        private void CloseActiveWindow()
        {
            if (ActiveMdiChild == null)
            {
                return;
            }

            string title = ActiveMdiChild.Text;
            ActiveMdiChild.Close();
            SetStatus($"Fenster geschlossen: {title}");
        }

        private void ApplyMdiLayout(MdiLayout layout, string statusText)
        {
            if (MdiChildren.Length == 0)
            {
                return;
            }

            LayoutMdi(layout);
            SetStatus(statusText);
        }

        private DirectoryWindowForm ActiveWindow => ActiveMdiChild as DirectoryWindowForm;

        private string GetCurrentDirectory()
        {
            return ActiveWindow?.CurrentPath ?? GetStartupPath();
        }

        private static string GetStartupPath()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (!string.IsNullOrWhiteSpace(desktop) && Directory.Exists(desktop))
            {
                return desktop;
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void SetStatus(string text)
        {
            _statusLabel.Text = string.IsNullOrWhiteSpace(text) ? "Bereit" : text;
        }

        private void UpdateWindowTitle()
        {
            // In Version 1.1 soll die Titelleiste bewusst stabil bleiben.
            // Der aktuell geöffnete Pfad gehört in die Arbeitsoberfläche,
            // nicht in die Haupttitelzeile des Produkts.
            Text = ProductInfo.MainWindowTitle;
        }

        private void RestoreWindowLayout()
        {
            WindowLayoutSettings layout = _appSettings?.MainWindow;
            if (layout == null)
            {
                ApplyDefaultWindowLayout();
                return;
            }

            Rectangle requestedBounds = layout.ToRectangle();
            Rectangle safeBounds = WindowLayoutHelper.GetSafeBounds(requestedBounds, MinimumSize, ProductInfo.DefaultMainWindowSize);
            Bounds = safeBounds;
            WindowState = layout.GetSafeWindowState();
            UpdateWindowTitle();
        }

        private void ApplyDefaultWindowLayout()
        {
            Rectangle defaultBounds = WindowLayoutHelper.GetCenteredPrimaryBounds(ProductInfo.DefaultMainWindowSize, MinimumSize);
            Bounds = defaultBounds;
            WindowState = FormWindowState.Maximized;
            UpdateWindowTitle();
        }

        private void SaveWindowLayout()
        {
            if (_appSettings == null)
            {
                return;
            }

            Rectangle boundsToPersist = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            _appSettings.MainWindow = new WindowLayoutSettings
            {
                X = boundsToPersist.X,
                Y = boundsToPersist.Y,
                Width = boundsToPersist.Width,
                Height = boundsToPersist.Height,
                WindowState = WindowState == FormWindowState.Minimized ? FormWindowState.Normal : WindowState
            };

            AppSettingsStore.Save(_appSettings);
        }
    }
}
