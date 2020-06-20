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
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }
    }
}
