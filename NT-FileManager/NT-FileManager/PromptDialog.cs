using System;
using System.Drawing;
using System.Windows.Forms;

namespace RetroNtFileManager
{
    internal sealed class PromptDialog : Form
    {
        private readonly TextBox _textBox;

        public string Value => _textBox.Text.Trim();

        public PromptDialog(string title, string label, string initialValue)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(360, 120);

            Label promptLabel = new Label
            {
                AutoSize = true,
                Left = 12,
                Top = 12,
                Text = label
            };

            _textBox = new TextBox
            {
                Left = 12,
                Top = 34,
                Width = 332,
                Text = initialValue ?? string.Empty
            };

            Button okButton = new Button
            {
                Text = "OK",
                Left = 188,
                Top = 72,
                Width = 75,
                DialogResult = DialogResult.OK
            };

            Button cancelButton = new Button
            {
                Text = "Abbrechen",
                Left = 269,
                Top = 72,
                Width = 75,
                DialogResult = DialogResult.Cancel
            };

            AcceptButton = okButton;
            CancelButton = cancelButton;

            Controls.Add(promptLabel);
            Controls.Add(_textBox);
            Controls.Add(okButton);
            Controls.Add(cancelButton);

            ClassicTheme.Apply(this);
        }
    }
}
