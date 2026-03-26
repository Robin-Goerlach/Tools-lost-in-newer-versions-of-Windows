using System.Drawing;
using System.Windows.Forms;

namespace RetroNtFileManager
{
    /// <summary>
    /// Anwendungseinstieg für den klassischen WinForms-Dateimanager.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Startet die Anwendung in einem STA-Thread und öffnet das Hauptfenster.
        /// </summary>
        [System.STAThread]
        /// <summary>
        /// Startet die Anwendung in einem STA-Thread und öffnet das Hauptfenster.
        /// </summary>
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    /// <summary>
    /// Bündelt das klassische, bewusst zurückhaltende Look-and-Feel des Projekts.
    /// </summary>
    internal static class ClassicTheme
    {
        /// <summary>
        /// Einheitliche UI-Schrift für den gesamten Client.
        /// </summary>
        public static readonly Font UiFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point);

        /// <summary>
        /// Wendet die klassische Systemdarstellung rekursiv auf ein Control und alle Unterelemente an.
        /// </summary>
        /// <param name="control">Wurzel-Control, ab dem die Formatierung angewendet wird.</param>
        /// <summary>
        /// Wendet die klassische Systemdarstellung rekursiv auf ein Control und alle Unterelemente an.
        /// </summary>
        /// <param name="control">Wurzel-Control, ab dem die Formatierung angewendet wird.</param>
        public static void Apply(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.Font = UiFont;

            // Eingabesteuerelemente behalten ihre systemtypischen Hintergrundfarben für bessere Lesbarkeit.
            if (!(control is TextBoxBase) &&
                !(control is TreeView) &&
                !(control is ListView) &&
                !(control is ComboBox))
            {
                control.BackColor = SystemColors.Control;
            }

            if (control is MenuStrip menuStrip)
            {
                menuStrip.RenderMode = ToolStripRenderMode.System;
                menuStrip.BackColor = SystemColors.Control;
            }

            if (control is ToolStrip toolStrip)
            {
                toolStrip.RenderMode = ToolStripRenderMode.System;
                toolStrip.BackColor = SystemColors.Control;
                toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            }

            if (control is StatusStrip statusStrip)
            {
                statusStrip.RenderMode = ToolStripRenderMode.System;
                statusStrip.BackColor = SystemColors.Control;
                statusStrip.SizingGrip = false;
            }

            // Kinder rekursiv angleichen, damit Dialoge und Unterfenster konsistent aussehen.
            foreach (Control child in control.Controls)
            {
                Apply(child);
            }
        }
    }
}
