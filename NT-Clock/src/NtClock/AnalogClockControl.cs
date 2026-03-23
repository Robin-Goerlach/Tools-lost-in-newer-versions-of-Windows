using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NtClock
{
    internal sealed class AnalogClockControl : Control
    {
        private static readonly Color MarkerColor = Color.Teal;
        private static readonly Color MinuteHandColor = Color.Teal;
        private static readonly Color HourHandColor = Color.FromArgb(0, 160, 160);
        private static readonly Color SecondHandColor = Color.Gray;

        public DateTime CurrentTime { get; set; } = DateTime.Now;
        public bool ShowSeconds { get; set; } = true;
        public bool SmoothSecondHand { get; set; } = false;

        public AnalogClockControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw,
                     true);

            BackColor = SystemColors.Control;
            Size = new Size(220, 220);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.Clear(BackColor);

            var rect = ClientRectangle;
            rect.Inflate(-8, -8);

            float cx = rect.Left + rect.Width / 2f;
            float cy = rect.Top + rect.Height / 2f;
            float radius = Math.Min(rect.Width, rect.Height) / 2f;

            DrawMarkers(g, cx, cy, radius);
            DrawHands(g, cx, cy, radius);
        }

        private void DrawMarkers(Graphics g, float cx, float cy, float radius)
        {
            using var markerBrush = new SolidBrush(MarkerColor);
            using var tinyBrush = new SolidBrush(Color.Gainsboro);

            for (int i = 0; i < 60; i++)
            {
                double angle = Math.PI * 2 * i / 60.0;
                float x = cx + (float)Math.Sin(angle) * radius * 0.92f;
                float y = cy - (float)Math.Cos(angle) * radius * 0.92f;

                if (i % 5 == 0)
                {
                    g.FillRectangle(markerBrush, x - 2, y - 2, 4, 4);
                }
                else
                {
                    g.FillRectangle(tinyBrush, x, y, 1, 1);
                }
            }
        }

        private void DrawHands(Graphics g, float cx, float cy, float radius)
        {
            var now = CurrentTime;

            double sec = SmoothSecondHand ? now.Second + now.Millisecond / 1000.0 : now.Second;
            double min = now.Minute + sec / 60.0;
            double hour = (now.Hour % 12) + min / 60.0;

            DrawHandPolygon(g, cx, cy, radius * 0.40f, hour / 12.0, 8f, HourHandColor);
            DrawHandPolygon(g, cx, cy, radius * 0.62f, min / 60.0, 6f, MinuteHandColor);

            if (ShowSeconds)
            {
                DrawNeedle(g, cx, cy, radius * 0.78f, sec / 60.0, SecondHandColor);
            }
        }

        private static void DrawHandPolygon(Graphics g, float cx, float cy, float length, double unit, float width, Color color)
        {
            double angle = unit * Math.PI * 2.0;
            float dx = (float)Math.Sin(angle);
            float dy = (float)-Math.Cos(angle);
            float px = -dy;
            float py = dx;

            PointF[] points =
            {
                new PointF(cx + px * width * 0.45f, cy + py * width * 0.45f),
                new PointF(cx + dx * length, cy + dy * length),
                new PointF(cx - px * width * 0.45f, cy - py * width * 0.45f),
                new PointF(cx - dx * length * 0.12f, cy - dy * length * 0.12f)
            };

            using var brush = new SolidBrush(color);
            g.FillPolygon(brush, points);
        }

        private static void DrawNeedle(Graphics g, float cx, float cy, float length, double unit, Color color)
        {
            double angle = unit * Math.PI * 2.0;
            float x = cx + (float)Math.Sin(angle) * length;
            float y = cy - (float)Math.Cos(angle) * length;

            using var pen = new Pen(color, 1f);
            g.DrawLine(pen, cx, cy, x, y);
        }
    }
}
