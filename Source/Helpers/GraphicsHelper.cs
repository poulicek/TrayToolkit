using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace TrayToolkit.Helpers
{
    public static class GraphicsHelper
    {
        public static void SetHighQuality(this Graphics g)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
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
    }
}
