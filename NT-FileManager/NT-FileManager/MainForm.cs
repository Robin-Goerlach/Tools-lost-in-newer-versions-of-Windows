using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RetroNtFileManager
{
    /// <summary>
    /// Hauptfenster der Anwendung. Es verwaltet die MDI-Kindfenster,
    /// stellt globale Menüs und Shortcuts bereit und speichert das Layout.
    /// </summary>
    internal sealed class MainForm : Form
    {
        /// <summary>
        /// Zentrale Menüleiste des Hauptfensters.
        /// </summary>
        private readonly MenuStrip _menuStrip;

        /// <summary>
        /// Textbasierte Werkzeugleiste für häufige Aktionen.
        /// </summary>
        private readonly ToolStrip _toolStrip;

        /// <summary>
        /// Statusleiste am unteren Rand des Hauptfensters.
        /// </summary>
        private readonly StatusStrip _statusStrip;

        /// <summary>
        /// Primäre Statusanzeige im Hauptfenster.
        /// </summary>
        private readonly ToolStripStatusLabel _statusLabel;

        /// <summary>
        /// Geladene Anwendungseinstellungen inklusive Layoutdaten.
        /// </summary>
        private readonly AppSettings _appSettings;

        /// <summary>
        /// Initialisiert das Hauptfenster und erstellt die komplette UI-Struktur.
        /// </summary>
        public MainForm()
        {
            // Einstellungen werden beim Start geladen, damit Layout und spätere Optionen sofort verfügbar sind.
            _appSettings = AppSettingsStore.Load();

            // Die Titelzeile bleibt bewusst stabil und zeigt keinen dynamischen Pfad an.
            Text = ProductInfo.MainWindowTitle;
            IsMdiContainer = true;
            StartPosition = FormStartPosition.Manual;
            MinimumSize = new Size(900, 600);
            BackColor = Color.Teal;
            KeyPreview = true;

            // Menü und Toolbar werden getrennt aufgebaut, damit dieselben Aktionen über mehrere Bedienwege erreichbar sind.
            _menuStrip = BuildMenuStrip();
            _toolStrip = BuildToolStrip();

            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Bereit");
            _statusStrip.Items.Add(_statusLabel);

            Controls.Add(_statusStrip);
            Controls.Add(_toolStrip);
            Controls.Add(_menuStrip);
            MainMenuStrip = _menuStrip;

            // Lebenszyklusereignisse werden zentral im Hauptfenster verdrahtet.
            Load += OnMainFormLoad;
            FormClosing += OnMainFormClosing;
            MdiChildActivate += (_, __) => UpdateWindowTitle();

            ClassicTheme.Apply(this);
        }

        /// <summary>
        /// Stellt beim ersten Anzeigen das Layout wieder her und öffnet das erste Datei-Fenster.
        /// </summary>
        private void OnMainFormLoad(object sender, EventArgs e)
        {
            RestoreWindowLayout();
            OpenNewWindow(GetStartupPath());
        }

        /// <summary>
        /// Speichert das Hauptfensterlayout vor dem Schließen.
        /// </summary>
        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowLayout();
        }

        /// <summary>
        /// Baut die komplette Menüleiste des Hauptfensters auf.
        /// </summary>
        private MenuStrip BuildMenuStrip()
        {
            MenuStrip menuStrip = new MenuStrip();

            // Datei-Menü mit den klassischen Kernaktionen des aktiven Datei-Fensters.
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

            // Datenträger-Menü bündelt Navigations- und Pfad-/Shell-Aktionen.
            ToolStripMenuItem diskMenu = new ToolStripMenuItem("&Datenträger");
            diskMenu.DropDownItems.Add(CreateItem("Nach oben", "Backspace", (_, __) => ActiveWindow?.GoUpOneLevel()));
            diskMenu.DropDownItems.Add(CreateItem("A&ktualisieren", "Ctrl+R", (_, __) => ActiveWindow?.RefreshView()));
            diskMenu.DropDownItems.Add(new ToolStripSeparator());
            diskMenu.DropDownItems.Add(CreateItem("Pfad als &Windows-Pfad kopieren", "Ctrl+Shift+C", (_, __) => ActiveWindow?.CopyCurrentPathAsWindowsPath()));
            diskMenu.DropDownItems.Add(CreateItem("Pfad als &Linux-/WSL-Pfad kopieren", "Ctrl+Shift+L", (_, __) => ActiveWindow?.CopyCurrentPathAsLinuxPath()));
            diskMenu.DropDownItems.Add(CreateItem("&CMD hier öffnen", "Ctrl+Shift+K", (_, __) => ActiveWindow?.OpenCommandPromptHere()));

            // Baum-Menü steuert die Ordnerstruktur links im Datei-Fenster.
            ToolStripMenuItem treeMenu = new ToolStripMenuItem("&Baum");
            treeMenu.DropDownItems.Add(CreateItem("Verzweigung &erweitern", "Right", (_, __) => ActiveWindow?.ExpandCurrentTreeNode()));
            treeMenu.DropDownItems.Add(CreateItem("Verzweigung &reduzieren", "Left", (_, __) => ActiveWindow?.CollapseCurrentTreeNode()));

            // Ansicht-Menü steuert die Listendarstellung.
            ToolStripMenuItem viewMenu = new ToolStripMenuItem("&Ansicht");
            viewMenu.DropDownItems.Add(CreateItem("&Große Symbole", "Ctrl+1", (_, __) => ActiveWindow?.SetView(View.LargeIcon)));
            viewMenu.DropDownItems.Add(CreateItem("&Kleine Symbole", "Ctrl+2", (_, __) => ActiveWindow?.SetView(View.SmallIcon)));
            viewMenu.DropDownItems.Add(CreateItem("&Liste", "Ctrl+3", (_, __) => ActiveWindow?.SetView(View.List)));
            viewMenu.DropDownItems.Add(CreateItem("&Details", "Ctrl+4", (_, __) => ActiveWindow?.SetView(View.Details)));
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(CreateItem("Aktualisieren", "Ctrl+R", (_, __) => ActiveWindow?.RefreshView()));

            // Optionen-Menü enthält bewusst nur wenige, klar verständliche Schalter.
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

            // Fenster-Menü bündelt MDI-spezifische Navigation und Layoutfunktionen.
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

        /// <summary>
        /// Baut die Toolbar für häufige Aktionen auf.
        /// </summary>
        private ToolStrip BuildToolStrip()
        {
            // Die Toolbar bleibt textbasiert, damit sie unabhängig von externen Icons stabil und lesbar ist.
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

        /// <summary>
        /// Erstellt einen Menüeintrag mit optionaler Shortcut-Anzeige.
        /// </summary>
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

        /// <summary>
        /// Erstellt einen textbasierten Toolbar-Button.
        /// </summary>
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

        /// <summary>
        /// Fängt globale Tastenkombinationen zentral ab.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (HandleGlobalShortcut(keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Ordnet Tastenkombinationen den jeweiligen Aktionen zu.
        /// </summary>
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

        /// <summary>
        /// Öffnet ein neues MDI-Kindfenster im angegebenen Verzeichnis.
        /// </summary>
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

        /// <summary>
        /// Öffnet den About-Dialog modal über dem Hauptfenster.
        /// </summary>
        private void ShowAboutDialog()
        {
            using (AboutDialog dialog = new AboutDialog())
            {
                dialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// Aktiviert das nächste MDI-Fenster.
        /// </summary>
        private void ActivateNextWindow()
        {
            CycleWindows(forward: true);
        }

        /// <summary>
        /// Aktiviert das vorherige MDI-Fenster.
        /// </summary>
        private void ActivatePreviousWindow()
        {
            CycleWindows(forward: false);
        }

        /// <summary>
        /// Durchläuft die offenen MDI-Fenster zyklisch vorwärts oder rückwärts.
        /// </summary>
        private void CycleWindows(bool forward)
        {
            // Das MDI-Framework liefert alle geöffneten Kindfenster in ihrer aktuellen Verwaltungsreihenfolge.
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

        /// <summary>
        /// Schließt das aktuell aktive Kindfenster.
        /// </summary>
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

        /// <summary>
        /// Wendet ein MDI-Layout an und schreibt eine passende Statusmeldung.
        /// </summary>
        private void ApplyMdiLayout(MdiLayout layout, string statusText)
        {
            if (MdiChildren.Length == 0)
            {
                return;
            }

            LayoutMdi(layout);
            SetStatus(statusText);
        }

        /// <summary>
        /// Liefert das aktuell aktive Datei-Fenster.
        /// </summary>
        private DirectoryWindowForm ActiveWindow => ActiveMdiChild as DirectoryWindowForm;

        /// <summary>
        /// Ermittelt das aktuelle Arbeitsverzeichnis des aktiven Fensters oder einen Fallback.
        /// </summary>
        private string GetCurrentDirectory()
        {
            return ActiveWindow?.CurrentPath ?? GetStartupPath();
        }

        /// <summary>
        /// Liefert einen sinnvollen Startpfad für neue Fenster.
        /// </summary>
        private static string GetStartupPath()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (!string.IsNullOrWhiteSpace(desktop) && Directory.Exists(desktop))
            {
                return desktop;
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>
        /// Schreibt eine Meldung in die Hauptstatusleiste.
        /// </summary>
        private void SetStatus(string text)
        {
            _statusLabel.Text = string.IsNullOrWhiteSpace(text) ? "Bereit" : text;
        }

        /// <summary>
        /// Hält die Haupttitelleiste auf dem festen Produktnamen.
        /// </summary>
        private void UpdateWindowTitle()
        {
            // In Version 1.1 soll die Titelleiste bewusst stabil bleiben.
            // Der aktuell geöffnete Pfad gehört in die Arbeitsoberfläche,
            // nicht in die Haupttitelzeile des Produkts.
            Text = ProductInfo.MainWindowTitle;
        }

        /// <summary>
        /// Stellt das gespeicherte Hauptfensterlayout wieder her.
        /// </summary>
        private void RestoreWindowLayout()
        {
            WindowLayoutSettings layout = _appSettings?.MainWindow;
            if (layout == null)
            {
                ApplyDefaultWindowLayout();
                return;
            }

            // Gespeicherte Werte werden zunächst normalisiert und anschließend auf Sichtbarkeit geprüft.
            Rectangle requestedBounds = layout.ToRectangle();
            Rectangle safeBounds = WindowLayoutHelper.GetSafeBounds(requestedBounds, MinimumSize, ProductInfo.DefaultMainWindowSize);
            Bounds = safeBounds;
            WindowState = layout.GetSafeWindowState();
            UpdateWindowTitle();
        }

        /// <summary>
        /// Wendet die Standardgeometrie für einen Erststart an.
        /// </summary>
        private void ApplyDefaultWindowLayout()
        {
            Rectangle defaultBounds = WindowLayoutHelper.GetCenteredPrimaryBounds(ProductInfo.DefaultMainWindowSize, MinimumSize);
            Bounds = defaultBounds;
            WindowState = FormWindowState.Maximized;
            UpdateWindowTitle();
        }

        /// <summary>
        /// Speichert Größe, Position und Zustand des Hauptfensters in den Einstellungen.
        /// </summary>
        private void SaveWindowLayout()
        {
            if (_appSettings == null)
            {
                return;
            }

            // Im maximierten Zustand muss RestoreBounds gespeichert werden, damit die normale Größe erhalten bleibt.
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
