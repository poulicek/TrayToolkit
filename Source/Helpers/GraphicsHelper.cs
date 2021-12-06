using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class GraphicsHelper
    {
        public static void SetQuality(this Graphics g, bool high)
        {
            if (high)
                g.SetHighQuality();
            else
                g.SetLowQuality();
        }

        public static void SetHighQuality(this Graphics g)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        public static void SetLowQuality(this Graphics g)
        {
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.TextRenderingHint = TextRenderingHint.SystemDefault;
        }

        public static Size Ceiling(this SizeF s)
        {
            return new Size((int)Math.Ceiling(s.Width), (int)Math.Ceiling(s.Height));
        }

        public static Point GetCenter(this Rectangle r)
        {
            return new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
        }

        public static Color SetAlphaChannel(this Color c, byte alpha)
        {
            int val = (alpha << 24) | (c.R << 16) | (c.G << 8) | c.B;
            return Color.FromArgb(val);
        }
    }
}
