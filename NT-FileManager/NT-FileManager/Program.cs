using System;
using System.Drawing;
using System.Windows.Forms;

namespace RetroNtFileManager
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    internal static class ClassicTheme
    {
        public static readonly Font UiFont = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point);

        public static void Apply(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.Font = UiFont;

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

            foreach (Control child in control.Controls)
            {
                Apply(child);
            }
        }
    }
}
