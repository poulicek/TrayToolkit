using System;
using System.Drawing;
using System.Windows.Forms;

namespace TrayToolkit.UI
{
    public class LabeledProgressBar : ProgressBar
    {
        public string Label { get; set; }

        public LabeledProgressBar()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // double-buffered drawing
            using (var buffer = new Bitmap(e.ClipRectangle.Width, e.ClipRectangle.Height))
            using (var g = Graphics.FromImage(buffer))
            {
                this.drawProgressBar(e.Graphics, new Rectangle(0, 0, buffer.Width, buffer.Height));
                e.Graphics.DrawImage(buffer, e.ClipRectangle);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {            
        }


        /// <summary>
        /// Draws the progress bar
        /// </summary>
        private void drawProgressBar(Graphics g, Rectangle rect)
        {
            g.Clear(this.BackColor);

            rect.Inflate(-1, -1);
            ProgressBarRenderer.DrawHorizontalBar(g, rect);

            if (this.Value > 0)
            {
                rect.Inflate(-1, -1);
                var clip = new Rectangle(rect.X, rect.Y, (int)Math.Round(((float)this.Value / this.Maximum) * rect.Width), rect.Height);
                ProgressBarRenderer.DrawHorizontalChunks(g, clip);
            }

            if (!string.IsNullOrEmpty(this.Label))
            {
                using (var f = new Font(FontFamily.GenericSansSerif, 8))
                using (var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(this.Label, f, Brushes.Black, this.ClientRectangle, sf);
            }
        }
    }
}
