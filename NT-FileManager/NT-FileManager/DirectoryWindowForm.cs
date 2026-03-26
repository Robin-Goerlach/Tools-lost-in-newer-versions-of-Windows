using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroNtFileManager
{
    internal sealed class DirectoryWindowForm : Form
    {
        private readonly Action<string> _statusCallback;
        private readonly ComboBox _driveComboBox;
        private readonly TextBox _pathTextBox;
        private readonly SplitContainer _splitContainer;
        private readonly TreeView _treeView;
        private readonly ListView _listView;
        private readonly ImageList _smallImageList;
        private readonly ImageList _largeImageList;
        private readonly ImageList _treeImageList;
        private readonly StatusStrip _statusStrip;
        private readonly ToolStripStatusLabel _leftStatus;
        private readonly ToolStripStatusLabel _rightStatus;
        private readonly FileListViewItemComparer _listComparer;
        private readonly ContextMenuStrip _contextMenu;

        private const string DragSourceWindowFormat = "SASD.RetroNtFileManager.SourceWindow";

        private bool _synchronizingTreeSelection;
        private bool _synchronizingDriveSelection;

        public DirectoryWindowForm(string initialPath, Action<string> statusCallback)
        {
            _statusCallback = statusCallback ?? (_ => { });

            Text = "Datei-Fenster";
            Width = 980;
            Height = 700;
            StartPosition = FormStartPosition.Manual;

            _smallImageList = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };
            _largeImageList = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(32, 32) };
            _treeImageList = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };
            LoadCommonIcons();

            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BorderStyle = BorderStyle.Fixed3D
            };

            Label driveLabel = new Label { Left = 8, Top = 9, Width = 60, Text = "Laufwerk:" };
            _driveComboBox = new ComboBox
            {
                Left = 70,
                Top = 6,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _driveComboBox.SelectedIndexChanged += DriveComboBox_SelectedIndexChanged;

            Label pathLabel = new Label { Left = 8, Top = 31, Width = 60, Text = "Pfad:" };
            _pathTextBox = new TextBox
            {
                Left = 70,
                Top = 28,
                Width = 860,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _pathTextBox.KeyDown += PathTextBox_KeyDown;

            topPanel.Controls.Add(driveLabel);
            topPanel.Controls.Add(_driveComboBox);
            topPanel.Controls.Add(pathLabel);
            topPanel.Controls.Add(_pathTextBox);

            _splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.Fixed3D,
                SplitterDistance = 280
            };

            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                HideSelection = false,
                ImageList = _treeImageList,
                BorderStyle = BorderStyle.Fixed3D
            };
            _treeView.BeforeExpand += TreeView_BeforeExpand;
            _treeView.AfterSelect += TreeView_AfterSelect;

            _listComparer = new FileListViewItemComparer();
            _listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = true,
                SmallImageList = _smallImageList,
                LargeImageList = _largeImageList,
                BorderStyle = BorderStyle.Fixed3D,
                ListViewItemSorter = _listComparer,
                AllowDrop = true
            };
            _listView.Columns.Add("Name", 240);
            _listView.Columns.Add("Erw.", 70);
            _listView.Columns.Add("Größe", 110, HorizontalAlignment.Right);
            _listView.Columns.Add("Typ", 140);
            _listView.Columns.Add("Geändert", 150);
            _listView.Columns.Add("Attribute", 90);
            _listView.ItemActivate += ListView_ItemActivate;
            _listView.ColumnClick += ListView_ColumnClick;
            _listView.SelectedIndexChanged += (_, __) => UpdateStatusBar();
            _listView.KeyDown += ListView_KeyDown;
            _listView.ItemDrag += ListView_ItemDrag;
            _listView.DragEnter += ListView_DragEnter;
            _listView.DragOver += ListView_DragOver;
            _listView.DragDrop += ListView_DragDrop;

            _contextMenu = BuildContextMenu();
            _listView.ContextMenuStrip = _contextMenu;
            _treeView.ContextMenuStrip = _contextMenu;

            _splitContainer.Panel1.Controls.Add(_treeView);
            _splitContainer.Panel2.Controls.Add(_listView);

            _statusStrip = new StatusStrip();
            _leftStatus = new ToolStripStatusLabel("0 Objekte") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _rightStatus = new ToolStripStatusLabel("Bereit");
            _statusStrip.Items.Add(_leftStatus);
            _statusStrip.Items.Add(_rightStatus);

            Controls.Add(_splitContainer);
            Controls.Add(_statusStrip);
            Controls.Add(topPanel);

            PopulateDriveList();
            LoadTreeRoots();

            ClassicTheme.Apply(this);

            string startPath = string.IsNullOrWhiteSpace(initialPath) || !Directory.Exists(initialPath)
                ? GetFirstAvailableDriveOrDocuments()
                : initialPath;
            NavigateTo(startPath, updateTreeSelection: true);
        }

        public string CurrentPath { get; private set; }

        public bool FullRowSelect
        {
            get => _listView.FullRowSelect;
            set => _listView.FullRowSelect = value;
        }

        private enum FileTransferMode
        {
            Copy,
            Move
        }

        public void OpenSelectedEntry()
        {
            List<string> selectedPaths = GetSelectedPaths();
            if (selectedPaths.Count == 0)
            {
                NavigateTo(CurrentPath, updateTreeSelection: true);
                return;
            }

            foreach (string path in selectedPaths)
            {
                if (Directory.Exists(path))
                {
                    NavigateTo(path, updateTreeSelection: true);
                    break;
                }

                OpenPath(path);
            }
        }

        public void CopySelectedEntry()
        {
            ExecuteSelectionOperation("Kopieren nach", selectedPaths =>
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Zielordner auswählen";
                    dialog.SelectedPath = CurrentPath;
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    foreach (string path in selectedPaths)
                    {
                        FileSystemOperations.CopyEntry(path, dialog.SelectedPath);
                    }

                    Status($"{selectedPaths.Count} Objekt(e) kopiert nach {dialog.SelectedPath}");
                }
            });
        }

        public void MoveSelectedEntry()
        {
            ExecuteSelectionOperation("Verschieben nach", selectedPaths =>
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Zielordner auswählen";
                    dialog.SelectedPath = CurrentPath;
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    foreach (string path in selectedPaths)
                    {
                        FileSystemOperations.MoveEntry(path, dialog.SelectedPath);
                    }

                    RefreshView();
                    Status($"{selectedPaths.Count} Objekt(e) verschoben nach {dialog.SelectedPath}");
                }
            });
        }

        public void RenameSelectedEntry()
        {
            List<string> selectedPaths = GetSelectedPaths();
            if (selectedPaths.Count != 1)
            {
                MessageBox.Show(this, "Zum Umbenennen muss genau ein Objekt ausgewählt sein.", "Umbenennen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string oldPath = selectedPaths[0];
            if (IsDriveRoot(oldPath))
            {
                MessageBox.Show(this, "Die Laufwerkswurzel kann nicht umbenannt werden.", "Umbenennen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string oldName = Path.GetFileName(oldPath.TrimEnd(Path.DirectorySeparatorChar));
            using (PromptDialog dialog = new PromptDialog("Umbenennen", "Neuen Namen eingeben:", oldName))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    FileSystemOperations.RenameEntry(oldPath, dialog.Value);
                    RefreshView();
                    SelectEntryByName(dialog.Value);
                    Status($"{oldName} umbenannt in {dialog.Value}");
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            }
        }

        public void DeleteSelectedEntry()
        {
            ExecuteSelectionOperation("Löschen", selectedPaths =>
            {
                DialogResult result = MessageBox.Show(
                    this,
                    $"{selectedPaths.Count} Objekt(e) wirklich löschen?",
                    "Löschen bestätigen",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                foreach (string path in selectedPaths)
                {
                    FileSystemOperations.DeleteEntry(path);
                }

                RefreshView();
                Status($"{selectedPaths.Count} Objekt(e) gelöscht");
            });
        }

        public void CreateFolder()
        {
            try
            {
                string folderPath = FileSystemOperations.CreateNewFolder(CurrentPath);
                RefreshView();
                SelectEntryByName(Path.GetFileName(folderPath));
                Status("Neuer Ordner erstellt");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        public void ShowPropertiesForSelection()
        {
            string path = GetSelectedPaths().FirstOrDefault() ?? CurrentPath;
            using (PropertiesDialog dialog = new PropertiesDialog(path))
            {
                dialog.ShowDialog(this);
            }
        }

        public void GoUpOneLevel()
        {
            if (string.IsNullOrWhiteSpace(CurrentPath))
            {
                return;
            }

            DirectoryInfo info = Directory.GetParent(CurrentPath);
            if (info != null)
            {
                NavigateTo(info.FullName, updateTreeSelection: true);
            }
        }

        public void RefreshView()
        {
            ReloadCurrentTreeNode();
            PopulateList(CurrentPath);
            UpdateStatusBar();
            Status("Ansicht aktualisiert");
        }

        public void CopyCurrentPathAsWindowsPath()
        {
            CopyPathToClipboard(CurrentPath, linuxStyle: false);
        }

        public void CopyCurrentPathAsLinuxPath()
        {
            CopyPathToClipboard(CurrentPath, linuxStyle: true);
        }

        public void CopySelectedEntryPathAsWindowsPath()
        {
            string path = GetSelectedPaths().FirstOrDefault() ?? CurrentPath;
            CopyPathToClipboard(path, linuxStyle: false);
        }

        public void CopySelectedEntryPathAsLinuxPath()
        {
            string path = GetSelectedPaths().FirstOrDefault() ?? CurrentPath;
            CopyPathToClipboard(path, linuxStyle: true);
        }

        public void OpenCommandPromptHere()
        {
            try
            {
                ShellLaunchHelper.OpenCommandPrompt(CurrentPath);
                Status($"CMD geöffnet: {CurrentPath}");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        public void ExpandCurrentTreeNode()
        {
            if (_treeView.SelectedNode == null)
            {
                return;
            }

            LoadChildNodes(_treeView.SelectedNode);
            _treeView.SelectedNode.Expand();
        }

        public void CollapseCurrentTreeNode()
        {
            _treeView.SelectedNode?.Collapse();
        }

        public void SetView(View view)
        {
            _listView.View = view;
            Status($"Ansicht: {view}");
        }

        private void NavigateTo(string path, bool updateTreeSelection)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                path = NormalizeDirectory(path);
                if (!Directory.Exists(path))
                {
                    ShowError($"Ordner wurde nicht gefunden:\n{path}");
                    return;
                }

                CurrentPath = path;
                Text = path;
                _pathTextBox.Text = path;
                PopulateList(path);
                SynchronizeDriveSelection(path);

                if (updateTreeSelection)
                {
                    EnsureTreeSelection(path);
                }

                UpdateStatusBar();
                Status(path);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void PopulateDriveList()
        {
            _driveComboBox.Items.Clear();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                _driveComboBox.Items.Add(drive.Name);
            }
        }

        private void LoadTreeRoots()
        {
            _treeView.BeginUpdate();
            _treeView.Nodes.Clear();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                TreeNode node = CreateDirectoryNode(drive.Name, drive.Name, isDrive: true);
                _treeView.Nodes.Add(node);
            }

            _treeView.EndUpdate();
        }

        private TreeNode CreateDirectoryNode(string text, string fullPath, bool isDrive = false)
        {
            string imageKey = isDrive ? "drive" : "folder";
            TreeNode node = new TreeNode(text)
            {
                Tag = fullPath,
                ImageKey = imageKey,
                SelectedImageKey = imageKey
            };
            node.Nodes.Add(new TreeNode());
            return node;
        }

        private void LoadChildNodes(TreeNode node)
        {
            string path = node.Tag as string;
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (node.Nodes.Count == 1 && string.IsNullOrWhiteSpace(node.Nodes[0].Text) && node.Nodes[0].Tag == null)
            {
                node.Nodes.Clear();
                try
                {
                    foreach (string directory in Directory.GetDirectories(path).OrderBy(d => d, StringComparer.CurrentCultureIgnoreCase))
                    {
                        string name = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar));
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            name = directory;
                        }

                        node.Nodes.Add(CreateDirectoryNode(name, directory));
                    }
                }
                catch
                {
                }
            }
        }

        private void PopulateList(string path)
        {
            _listView.BeginUpdate();
            _listView.Items.Clear();

            try
            {
                foreach (string directory in Directory.GetDirectories(path).OrderBy(d => d, StringComparer.CurrentCultureIgnoreCase))
                {
                    DirectoryInfo info = new DirectoryInfo(directory);
                    ListViewItem item = new ListViewItem(info.Name)
                    {
                        ImageKey = EnsureImageKey(directory, isDirectory: true),
                        Tag = new FileEntryInfo
                        {
                            FullPath = info.FullName,
                            Kind = "Directory",
                            Size = 0,
                            Modified = info.LastWriteTime,
                            Attributes = info.Attributes.ToString()
                        }
                    };
                    item.SubItems.Add(string.Empty);
                    item.SubItems.Add(string.Empty);
                    item.SubItems.Add("Dateiordner");
                    item.SubItems.Add(info.LastWriteTime.ToString("dd.MM.yyyy HH:mm"));
                    item.SubItems.Add(info.Attributes.ToString());
                    _listView.Items.Add(item);
                }

                foreach (string file in Directory.GetFiles(path).OrderBy(f => f, StringComparer.CurrentCultureIgnoreCase))
                {
                    FileInfo info = new FileInfo(file);
                    string extension = info.Extension.StartsWith(".") ? info.Extension.Substring(1).ToUpperInvariant() : info.Extension.ToUpperInvariant();
                    ListViewItem item = new ListViewItem(info.Name)
                    {
                        ImageKey = EnsureImageKey(file, isDirectory: false),
                        Tag = new FileEntryInfo
                        {
                            FullPath = info.FullName,
                            Kind = "File",
                            Size = info.Length,
                            Modified = info.LastWriteTime,
                            Attributes = info.Attributes.ToString()
                        }
                    };
                    item.SubItems.Add(extension);
                    item.SubItems.Add(info.Length.ToString("N0"));
                    item.SubItems.Add(string.IsNullOrWhiteSpace(extension) ? "Datei" : extension + "-Datei");
                    item.SubItems.Add(info.LastWriteTime.ToString("dd.MM.yyyy HH:mm"));
                    item.SubItems.Add(info.Attributes.ToString());
                    _listView.Items.Add(item);
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("Zugriff verweigert.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                _listView.EndUpdate();
            }
        }

        private string EnsureImageKey(string path, bool isDirectory)
        {
            string key = isDirectory
                ? "folder"
                : (Path.GetExtension(path) ?? string.Empty).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(key))
            {
                key = isDirectory ? "folder" : "file";
            }

            if (!_smallImageList.Images.ContainsKey(key))
            {
                _smallImageList.Images.Add(key, ShellIconHelper.GetSmallIcon(path, isDirectory));
            }

            if (!_largeImageList.Images.ContainsKey(key))
            {
                _largeImageList.Images.Add(key, ShellIconHelper.GetLargeIcon(path, isDirectory));
            }

            return key;
        }

        private void LoadCommonIcons()
        {
            _smallImageList.Images.Add("folder", ShellIconHelper.GetSmallIcon(Environment.SystemDirectory, true));
            _smallImageList.Images.Add("file", ShellIconHelper.GetSmallIcon("dummy.txt", false));
            _smallImageList.Images.Add("drive", ShellIconHelper.GetSmallIcon("C:\\", true));

            _largeImageList.Images.Add("folder", ShellIconHelper.GetLargeIcon(Environment.SystemDirectory, true));
            _largeImageList.Images.Add("file", ShellIconHelper.GetLargeIcon("dummy.txt", false));
            _largeImageList.Images.Add("drive", ShellIconHelper.GetLargeIcon("C:\\", true));

            _treeImageList.Images.Add("folder", ShellIconHelper.GetSmallIcon(Environment.SystemDirectory, true));
            _treeImageList.Images.Add("drive", ShellIconHelper.GetSmallIcon("C:\\", true));
        }

        private ContextMenuStrip BuildContextMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Öffnen", null, (_, __) => OpenSelectedEntry());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Kopieren...", null, (_, __) => CopySelectedEntry());
            menu.Items.Add("Verschieben...", null, (_, __) => MoveSelectedEntry());
            menu.Items.Add("Umbenennen", null, (_, __) => RenameSelectedEntry());
            menu.Items.Add("Löschen", null, (_, __) => DeleteSelectedEntry());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Pfad als Windows-Pfad kopieren", null, (_, __) => CopySelectedEntryPathAsWindowsPath());
            menu.Items.Add("Pfad als Linux-/WSL-Pfad kopieren", null, (_, __) => CopySelectedEntryPathAsLinuxPath());
            menu.Items.Add("CMD hier öffnen", null, (_, __) => OpenCommandPromptHere());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Eigenschaften", null, (_, __) => ShowPropertiesForSelection());
            return menu;
        }

        private void SynchronizeDriveSelection(string path)
        {
            string root = Path.GetPathRoot(path);
            if (string.IsNullOrWhiteSpace(root))
            {
                return;
            }

            _synchronizingDriveSelection = true;
            try
            {
                _driveComboBox.SelectedItem = root;
            }
            finally
            {
                _synchronizingDriveSelection = false;
            }
        }

        private void EnsureTreeSelection(string path)
        {
            string normalized = NormalizeDirectory(path);
            string root = Path.GetPathRoot(normalized);
            if (string.IsNullOrWhiteSpace(root))
            {
                return;
            }

            TreeNode current = null;
            foreach (TreeNode rootNode in _treeView.Nodes)
            {
                if (string.Equals(NormalizeDirectory(rootNode.Tag as string), NormalizeDirectory(root), StringComparison.OrdinalIgnoreCase))
                {
                    current = rootNode;
                    break;
                }
            }

            if (current == null)
            {
                return;
            }

            string relative = normalized.Substring(root.Length).Trim(Path.DirectorySeparatorChar);
            if (!string.IsNullOrWhiteSpace(relative))
            {
                foreach (string segment in relative.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
                {
                    LoadChildNodes(current);
                    TreeNode next = null;
                    foreach (TreeNode child in current.Nodes)
                    {
                        if (string.Equals(child.Text, segment, StringComparison.CurrentCultureIgnoreCase))
                        {
                            next = child;
                            break;
                        }
                    }

                    if (next == null)
                    {
                        break;
                    }

                    current.Expand();
                    current = next;
                }
            }

            _synchronizingTreeSelection = true;
            try
            {
                _treeView.SelectedNode = current;
                current.EnsureVisible();
            }
            finally
            {
                _synchronizingTreeSelection = false;
            }
        }

        private void ReloadCurrentTreeNode()
        {
            if (_treeView.SelectedNode == null)
            {
                return;
            }

            string path = _treeView.SelectedNode.Tag as string;
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            _treeView.SelectedNode.Nodes.Clear();
            _treeView.SelectedNode.Nodes.Add(new TreeNode());
            LoadChildNodes(_treeView.SelectedNode);
        }

        private void CopyPathToClipboard(string path, bool linuxStyle)
        {
            try
            {
                string normalizedPath = linuxStyle
                    ? PathClipboardHelper.ToLinuxPath(path)
                    : PathClipboardHelper.ToWindowsPath(path);

                if (string.IsNullOrWhiteSpace(normalizedPath))
                {
                    ShowError("Es konnte kein Pfad in die Zwischenablage kopiert werden.");
                    return;
                }

                Clipboard.SetText(normalizedPath);
                Status($"Pfad kopiert: {normalizedPath}");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private List<string> GetSelectedPaths()
        {
            List<string> selected = new List<string>();

            if (_listView.Focused && _listView.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in _listView.SelectedItems)
                {
                    if (item.Tag is FileEntryInfo info && !string.IsNullOrWhiteSpace(info.FullPath))
                    {
                        selected.Add(info.FullPath);
                    }
                }

                return selected;
            }

            if (_treeView.Focused && _treeView.SelectedNode?.Tag is string treePath)
            {
                selected.Add(treePath);
            }

            return selected;
        }

        private void ExecuteSelectionOperation(string title, Action<List<string>> action)
        {
            List<string> selectedPaths = GetSelectedPaths();
            if (selectedPaths.Count == 0)
            {
                MessageBox.Show(this, "Bitte zuerst ein Objekt auswählen.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (selectedPaths.Any(IsDriveRoot))
            {
                MessageBox.Show(this, "Die Laufwerkswurzel kann mit dieser Funktion nicht bearbeitet werden.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                action(selectedPaths);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void SelectEntryByName(string name)
        {
            foreach (ListViewItem item in _listView.Items)
            {
                if (string.Equals(item.Text, name, StringComparison.CurrentCultureIgnoreCase))
                {
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    _listView.Focus();
                    return;
                }
            }
        }

        private void OpenPath(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
                Status($"Geöffnet: {path}");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            string[] selectedPaths = GetSelectedPaths().Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (selectedPaths.Length == 0)
            {
                return;
            }

            DataObject dataObject = new DataObject();
            dataObject.SetData(DataFormats.FileDrop, selectedPaths);
            dataObject.SetData(DragSourceWindowFormat, this);

            DoDragDrop(dataObject, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = ResolveDropEffect(e);
        }

        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = ResolveDropEffect(e);
        }

        private void ListView_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (!TryGetDroppedPaths(e.Data, out string[] droppedPaths))
                {
                    return;
                }

                Point clientPoint = _listView.PointToClient(new Point(e.X, e.Y));
                string targetDirectory = GetDropTargetDirectory(clientPoint);
                if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
                {
                    return;
                }

                FileTransferMode transferMode = ResolveTransferMode(droppedPaths, targetDirectory, e.KeyState);
                foreach (string droppedPath in droppedPaths)
                {
                    if (transferMode == FileTransferMode.Move)
                    {
                        FileSystemOperations.MoveEntry(droppedPath, targetDirectory);
                    }
                    else
                    {
                        FileSystemOperations.CopyEntry(droppedPath, targetDirectory);
                    }
                }

                RefreshView();

                if (e.Data.GetDataPresent(DragSourceWindowFormat) && e.Data.GetData(DragSourceWindowFormat) is DirectoryWindowForm sourceWindow)
                {
                    if (!ReferenceEquals(sourceWindow, this))
                    {
                        sourceWindow.RefreshView();
                    }
                    else if (transferMode == FileTransferMode.Move)
                    {
                        sourceWindow.RefreshView();
                    }
                }

                string actionText = transferMode == FileTransferMode.Move ? "verschoben" : "kopiert";
                Status($"{droppedPaths.Length} Objekt(e) per Drag & Drop {actionText} nach {targetDirectory}");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private DragDropEffects ResolveDropEffect(DragEventArgs e)
        {
            if (!TryGetDroppedPaths(e.Data, out string[] droppedPaths))
            {
                return DragDropEffects.None;
            }

            Point clientPoint = _listView.PointToClient(new Point(e.X, e.Y));
            string targetDirectory = GetDropTargetDirectory(clientPoint);
            if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
            {
                return DragDropEffects.None;
            }

            foreach (string droppedPath in droppedPaths)
            {
                if (!CanDropPathToTarget(droppedPath, targetDirectory))
                {
                    return DragDropEffects.None;
                }
            }

            FileTransferMode transferMode = ResolveTransferMode(droppedPaths, targetDirectory, e.KeyState);
            return transferMode == FileTransferMode.Move ? DragDropEffects.Move : DragDropEffects.Copy;
        }

        private string GetDropTargetDirectory(Point clientPoint)
        {
            ListViewItem hoveredItem = _listView.GetItemAt(clientPoint.X, clientPoint.Y);
            if (hoveredItem?.Tag is FileEntryInfo hoveredInfo &&
                string.Equals(hoveredInfo.Kind, "Directory", StringComparison.OrdinalIgnoreCase) &&
                Directory.Exists(hoveredInfo.FullPath))
            {
                return hoveredInfo.FullPath;
            }

            return CurrentPath;
        }

        private static bool TryGetDroppedPaths(IDataObject dataObject, out string[] droppedPaths)
        {
            droppedPaths = null;

            if (dataObject == null || !dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                return false;
            }

            droppedPaths = dataObject.GetData(DataFormats.FileDrop) as string[];
            return droppedPaths != null && droppedPaths.Length > 0;
        }

        private static bool CanDropPathToTarget(string sourcePath, string targetDirectory)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || string.IsNullOrWhiteSpace(targetDirectory))
            {
                return false;
            }

            bool sourceExists = File.Exists(sourcePath) || Directory.Exists(sourcePath);
            if (!sourceExists || !Directory.Exists(targetDirectory))
            {
                return false;
            }

            string normalizedTarget = NormalizeDirectory(targetDirectory);
            string sourceParent = Path.GetDirectoryName(sourcePath);
            if (!string.IsNullOrWhiteSpace(sourceParent) &&
                string.Equals(NormalizeDirectory(sourceParent), normalizedTarget, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (Directory.Exists(sourcePath))
            {
                string normalizedSource = NormalizeDirectory(sourcePath);
                if (string.Equals(normalizedSource, normalizedTarget, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (normalizedTarget.StartsWith(normalizedSource + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            string targetPath = Path.Combine(targetDirectory, Path.GetFileName(sourcePath));
            if (string.Equals(Path.GetFullPath(sourcePath), Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static FileTransferMode ResolveTransferMode(IEnumerable<string> sourcePaths, string targetDirectory, int keyState)
        {
            const int MkControl = 0x0008;
            const int MkShift = 0x0004;

            if ((keyState & MkControl) == MkControl)
            {
                return FileTransferMode.Copy;
            }

            if ((keyState & MkShift) == MkShift)
            {
                return FileTransferMode.Move;
            }

            string targetRoot = Path.GetPathRoot(targetDirectory) ?? string.Empty;
            bool sameRoot = sourcePaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .All(path => string.Equals(Path.GetPathRoot(path) ?? string.Empty, targetRoot, StringComparison.OrdinalIgnoreCase));

            return sameRoot ? FileTransferMode.Move : FileTransferMode.Copy;
        }

        private void UpdateStatusBar()
        {
            int total = _listView.Items.Count;
            int selected = _listView.SelectedItems.Count;
            long selectedBytes = 0;

            foreach (ListViewItem item in _listView.SelectedItems)
            {
                if (item.Tag is FileEntryInfo info)
                {
                    selectedBytes += info.Size;
                }
            }

            _leftStatus.Text = selected > 0
                ? $"{selected} von {total} Objekt(en) ausgewählt"
                : $"{total} Objekt(e)";
            _rightStatus.Text = selected > 0 ? $"{selectedBytes:N0} Bytes ausgewählt" : CurrentPath;
        }

        private void Status(string text)
        {
            _statusCallback(text);
            _rightStatus.Text = text;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(this, message, ProductInfo.ProductDisplayName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Status(message);
        }

        private static bool IsDriveRoot(string path)
        {
            string normalized = NormalizeDirectory(path);
            string root = NormalizeDirectory(Path.GetPathRoot(normalized));
            return string.Equals(normalized, root, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeDirectory(string path)
        {
            string fullPath = Path.GetFullPath(path);
            if (fullPath.Length > 3)
            {
                return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            return fullPath;
        }

        private static string GetFirstAvailableDriveOrDocuments()
        {
            DriveInfo drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady);
            if (drive != null)
            {
                return drive.Name;
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private void DriveComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_synchronizingDriveSelection || _driveComboBox.SelectedItem == null)
            {
                return;
            }

            string selectedDrive = _driveComboBox.SelectedItem.ToString();
            NavigateTo(selectedDrive, updateTreeSelection: true);
        }

        private void PathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                NavigateTo(_pathTextBox.Text.Trim(), updateTreeSelection: true);
            }
        }

        private void TreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            LoadChildNodes(e.Node);
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_synchronizingTreeSelection)
            {
                return;
            }

            if (e.Node?.Tag is string path)
            {
                NavigateTo(path, updateTreeSelection: false);
            }
        }

        private void ListView_ItemActivate(object sender, EventArgs e)
        {
            OpenSelectedEntry();
        }

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_listComparer.ColumnIndex == e.Column)
            {
                _listComparer.SortOrder = _listComparer.SortOrder == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                _listComparer.ColumnIndex = e.Column;
                _listComparer.SortOrder = SortOrder.Ascending;
            }

            _listView.Sort();
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.Shift && e.KeyCode == Keys.C)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CopySelectedEntryPathAsWindowsPath();
                return;
            }

            if (e.Control && e.Shift && e.KeyCode == Keys.L)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CopySelectedEntryPathAsLinuxPath();
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Enter:
                    e.Handled = true;
                    OpenSelectedEntry();
                    break;
                case Keys.Delete:
                    e.Handled = true;
                    DeleteSelectedEntry();
                    break;
                case Keys.F2:
                    e.Handled = true;
                    RenameSelectedEntry();
                    break;
                case Keys.F5:
                    e.Handled = true;
                    RefreshView();
                    break;
                case Keys.Back:
                    e.Handled = true;
                    GoUpOneLevel();
                    break;
            }
        }
    }
}
