using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NtClock;

public sealed class AnalogClockControl : Control
{
    public DateTime Time { get; set; } = DateTime.Now;
    public bool ShowSeconds { get; set; } = true;
    public bool UseRedSecondHand { get; set; } = true;
    public bool SmoothSecondHand { get; set; } = false;

    public AnalogClockControl()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = SystemColors.Window;
        Size = new Size(164, 164);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.None;
        g.PixelOffsetMode = PixelOffsetMode.Half;

        Rectangle rect = ClientRectangle;
        rect.Inflate(-8, -8);

        using (var face = new SolidBrush(SystemColors.Window))
        using (var border = new Pen(SystemColors.ControlText, 2f))
        {
            g.FillEllipse(face, rect);
            g.DrawEllipse(border, rect);
        }

        float cx = rect.Left + rect.Width / 2f;
        float cy = rect.Top + rect.Height / 2f;
        float radius = Math.Min(rect.Width, rect.Height) / 2f;

        using (var majorPen = new Pen(SystemColors.ControlText, 1.6f))
        using (var minorPen = new Pen(SystemColors.ControlText, 1.0f))
        {
            for (int i = 0; i < 60; i++)
            {
                bool major = i % 5 == 0;
                float angle = (float)(Math.PI * 2 * i / 60.0);
                float r1 = radius * (major ? 0.80f : 0.88f);
                float r2 = radius * 0.95f;

                float x1 = cx + (float)Math.Sin(angle) * r1;
                float y1 = cy - (float)Math.Cos(angle) * r1;
                float x2 = cx + (float)Math.Sin(angle) * r2;
                float y2 = cy - (float)Math.Cos(angle) * r2;

                g.DrawLine(major ? majorPen : minorPen, x1, y1, x2, y2);
            }
        }

        DateTime now = Time;
        double sec = SmoothSecondHand ? (now.Second + now.Millisecond / 1000.0) : now.Second;
        double min = now.Minute + sec / 60.0;
        double hour = (now.Hour % 12) + min / 60.0;

        DrawHand(g, cx, cy, radius * 0.55f, hour / 12.0, 5f, SystemColors.ControlText);
        DrawHand(g, cx, cy, radius * 0.75f, min / 60.0, 3.5f, SystemColors.ControlText);

        if (ShowSeconds)
        {
            Color secColor = UseRedSecondHand ? Color.DarkRed : SystemColors.ControlText;
            DrawHand(g, cx, cy, radius * 0.82f, sec / 60.0, 1.5f, secColor);
        }

        using var cap = new SolidBrush(SystemColors.ControlText);
        g.FillEllipse(cap, cx - 4, cy - 4, 8, 8);
    }

    private static void DrawHand(Graphics g, float cx, float cy, float length, double unit, float thickness, Color color)
    {
        double angle = unit * 2.0 * Math.PI;
        float x = cx + (float)Math.Sin(angle) * length;
        float y = cy - (float)Math.Cos(angle) * length;

        using var pen = new Pen(color, thickness)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        };

        g.DrawLine(pen, cx, cy, x, y);
    }
}
