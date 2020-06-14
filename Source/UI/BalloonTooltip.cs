using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TrayToolkit.UI
{
    public static class BalloonTooltip
    {
        #region Balloon Control

        private class BalloonControl : Form
        {
            #region P/Invoke

            private const int SW_SHOWNOACTIVATE = 4;
            private const uint SWP_NOACTIVATE = 0x10;

            [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
            private static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
            [DllImport("user32.dll")]
            private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


            #endregion

            public new Bitmap Icon { get; set; }
            public string Message { get; set; }
            public string Note { get; set; }

            public BalloonControl()
            {
                this.BackColor = Color.FromArgb(36, 36, 36);
                this.ClientSize = new Size(364, 102);
                this.StartPosition = FormStartPosition.Manual;
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.TopMost = true;
                this.DoubleBuffered = true;

                this.AutoScaleDimensions = new SizeF(96F, 96F);
                this.AutoScaleMode = AutoScaleMode.Dpi;
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // hiding if no contents are available
                if (string.IsNullOrEmpty(this.Message))
                    this.Hide();
                else
                    base.OnPaintBackground(e);

            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (!string.IsNullOrEmpty(this.Message))
                    this.drawContents(e.Graphics);
            }


            /// <summary>
            /// Gets the desired location
            /// </summary>
            private Point getCornerLocation()
            {
                var screenArea = Screen.GetWorkingArea(this);
                return new Point(screenArea.Width - 24 - this.Width, screenArea.Height - 16 - this.Height);
            }


            /// <summary>
            /// Shows the unfocused contorl
            /// </summary>
            public void ShowUnfocused()
            {
                var loc = this.getCornerLocation();
                SetWindowPos(this.Handle.ToInt32(), 0, loc.X, loc.Y, this.Width, this.Height, SWP_NOACTIVATE);
                ShowWindow(this.Handle, SW_SHOWNOACTIVATE);
                this.Invalidate();
            }

            #region Look


            /// <summary>
            /// Draws the contents
            /// </summary>
            private void drawContents(Graphics g)
            {
                var margin = (int)(16 * g.DpiX / 96);
                var textRect = Rectangle.Inflate(this.ClientRectangle, -margin, -margin);
                var iconRect = this.drawIcon(g, textRect, margin);
                var textMargin = iconRect.IsEmpty ? 0 : iconRect.Width + margin;

                this.drawMessage(g, textRect, textMargin);
            }


            /// <summary>
            /// Draws the message
            /// </summary>
            private Rectangle drawMessage(Graphics g, Rectangle rect, int margin)
            {
                if (string.IsNullOrEmpty(this.Message))
                    return Rectangle.Empty;

                rect.X += margin;
                rect.Width -= margin;

                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.SmoothingMode = SmoothingMode.HighQuality;

                if (string.IsNullOrEmpty(this.Note))
                {
                    using (var f = new Font("Segoe UI", 11, FontStyle.Bold, GraphicsUnit.Point))
                    using (var sf = new StringFormat() { LineAlignment = StringAlignment.Center })
                    {
                        g.DrawString(this.Message, f, Brushes.White, rect, sf);
                        this.ensureSize(g, this.Message, f, rect);
                    }
                }
                else
                {
                    using (var f = new Font("Segoe UI", 11, FontStyle.Bold, GraphicsUnit.Point))
                    using (var sf = new StringFormat() { LineAlignment = StringAlignment.Near })
                    {
                        g.DrawString(this.Message, f, Brushes.White, new Rectangle(rect.X, rect.Y, rect.Width, 2 * rect.Height / 3), sf);
                        this.ensureSize(g, this.Message, f, rect);
                    }

                    using (var f = new Font("Segoe UI", 9, GraphicsUnit.Point))
                    using (var sf = new StringFormat() { LineAlignment = StringAlignment.Far })
                    {
                        g.DrawString(this.Note, f, Brushes.White, new Rectangle(rect.X, rect.Y + 2 * rect.Height / 3, rect.Width, rect.Height / 3), sf);
                        this.ensureSize(g, this.Note, f, rect);
                    }
                }

                return rect;
            }

            private void ensureSize(Graphics g, string str, Font f, Rectangle rect)
            {
                var s = g.MeasureString(str, f);
                if (s.Width > rect.Width)
                {
                    this.Width += ((int)Math.Ceiling(s.Width) - rect.Width);
                    this.ShowUnfocused();
                }
            }


            /// <summary>
            /// Draws the icon
            /// </summary>
            private Rectangle drawIcon(Graphics g, Rectangle rect, int margin)
            {
                if (this.Icon == null)
                    return Rectangle.Empty;

                var height = rect.Height - margin - margin;
                var width = this.Icon.Width * height / this.Icon.Height;
                var iconRect = new Rectangle(rect.X, rect.Y + margin, width, height);

                g.DrawImage(this.Icon, iconRect);

                return iconRect;
            }

            #endregion
        }

        #endregion

        private static BalloonControl tooltip = new BalloonControl();
        private static readonly Timer timer = new Timer();
        

        static BalloonTooltip()
        {
            timer.Tick += onTimerTick;
        }


        /// <summary>
        /// Resets the timer
        /// </summary>
        private static void resetTimer(int timeout = 0)
        {
            timer.Stop();

            if (timeout > 0)
            {
                timer.Interval = timeout;
                timer.Start();
            }
        }


        /// <summary>
        /// Initializes the tooltip activation by showing an empty balloon once
        /// </summary>
        public static void InitActivation()
        {
            tooltip.ShowUnfocused();
        }


        /// <summary>
        /// Shows the notification for period when the worker is running
        /// </summary>
        public static void Show(string message, Bitmap icon, Action worker)
        {
            try
            {
                Show(message, icon);
                worker?.Invoke();
            }
            finally { Hide(); }
        }


        /// <summary>
        /// Shows the notification
        /// </summary>
        public static void Show(string message, Bitmap icon = null, string note = null, int timeout = 0)
        {
            resetTimer(timeout);

            tooltip.Icon = icon;
            tooltip.Message = message;
            tooltip.Note = note;
            tooltip.ShowUnfocused();
        }


        /// <summary>
        /// Hides the notification
        /// </summary>
        public static void Hide()
        {
            resetTimer();
            tooltip.Hide();
        }


        /// <summary>
        /// Activates the tooltip control
        /// </summary>
        public static void Activate()
        {
            tooltip.Activate();
        }

        private static void onTimerTick(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
