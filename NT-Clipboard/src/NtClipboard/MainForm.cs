namespace NtClipboard;

public sealed class MainForm : Form
{
    public MainForm()
    {
        Text = "NT-Clipboard — Concept";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new System.Drawing.Size(520, 180);
        Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Text = "NT-Clipboard\nConceptual project scaffold — no utility features implemented yet."
        });
    }
}
