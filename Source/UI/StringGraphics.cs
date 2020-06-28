using System;
using System.Drawing;
using TrayToolkit.Helpers;

namespace TrayToolkit.UI
{
    internal class StringGraphics : IDisposable
    {
        public string Text { get; set; }
        public StringAlignment LineAlignment { get { return this.format.LineAlignment; } set { this.format.LineAlignment = value; } }

        public bool IsEmpty { get { return string.IsNullOrEmpty(this.Text); } }

        private readonly Font font;
        private readonly StringFormat format;

        public StringGraphics(Font font, StringFormat sf)
        {
            this.font = font;
            this.format = sf;
        }


        public void Draw(Graphics g, Brush brush, Rectangle area)
        {
            if (string.IsNullOrEmpty(this.Text))
                return;

            g.DrawString(this.Text, this.font, brush, area, this.format);
        }


        public Size Measure(int width, Graphics g = null)
        {
            if (string.IsNullOrEmpty(this.Text))
                return Size.Empty;

            if (g != null)
                return g.MeasureString(this.Text, this.font, width, this.format).Ceiling();

            using (var tmpBmp = new Bitmap(1, 1))
            using (g =  Graphics.FromImage(tmpBmp))
                return g.MeasureString(this.Text, this.font, width, this.format).Ceiling();
        }


        public void Dispose()
        {
            this.font?.Dispose();
            this.format?.Dispose();
        }
    }
}
