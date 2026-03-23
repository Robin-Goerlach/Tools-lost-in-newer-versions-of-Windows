using System;
using System.Drawing;
using System.Windows.Forms;

namespace NtClock;

public sealed class DigitalClassicControl : Control
{
    public DateTime Time { get; set; } = DateTime.Now;
    public bool Use24Hour { get; set; } = true;
    public bool ShowSeconds { get; set; } = true;

    public DigitalClassicControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = SystemColors.Control;
        ForeColor = SystemColors.ControlText;
        Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular);
        Size = new Size(164, 24);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColor);
        ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);

        Rectangle inner = ClientRectangle;
        inner.Inflate(-4, -3);

        string text = Use24Hour
            ? Time.ToString(ShowSeconds ? "HH:mm:ss" : "HH:mm")
            : Time.ToString(ShowSeconds ? "hh:mm:ss tt" : "hh:mm tt");

        TextRenderer.DrawText(
            e.Graphics,
            text,
            Font,
            inner,
            ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
    }
}
