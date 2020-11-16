using System;
using System.Drawing;
using System.Windows.Forms;
using TrayToolkit.Helpers;

namespace TrayToolkit.UI
{
    public static class BalloonTooltip
    {
        #region Balloon Control

        private class BalloonControl : Form
        {
            public new Bitmap Icon { get; set; }
            public string Message { get { return this.sgMessage.Text; } set { this.sgMessage.Text = value; } }
            public string Note { get { return this.sgNote.Text; } set { this.sgNote.Text = value; } }

            private readonly StringGraphics sgMessage = new StringGraphics(
                new Font("Segoe UI", 11, FontStyle.Bold, GraphicsUnit.Point),
                new StringFormat() { Trimming = StringTrimming.EllipsisCharacter });

            private readonly StringGraphics sgNote = new StringGraphics(
                new Font("Segoe UI", 9, GraphicsUnit.Point),
                new StringFormat() { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter });


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
                this.drawContents(e.Graphics);
            }


            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                this.sgMessage.Dispose();
                this.sgNote.Dispose();
            }


            /// <summary>
            /// Shows the unfocused contorl
            /// </summary>
            public void ShowInCorner()
            {
                this.ShowUnfocused();
                this.Location = this.GetCornerLocation();
            }

            #region Look

            /// <summary>
            /// Draws the contents
            /// </summary>
            private void drawContents(Graphics g)
            {
                var padding = (int)(16 * g.DpiX / 96);
                var clientHeight = (int)(102 * g.DpiX / 96);
                var cntRect = Rectangle.Inflate(this.ClientRectangle, -padding, -padding);

                var iconRect = new Rectangle(cntRect.X, cntRect.Y + padding, cntRect.Width, cntRect.Height - padding - padding);
                var iconSize = this.drawIcon(g, iconRect);

                var textMargin = iconSize.IsEmpty ? 0 : iconSize.Width + padding;
                var textRect = new Rectangle(cntRect.X + textMargin, cntRect.Y, cntRect.Width - textMargin, cntRect.Height);

                this.drawMessage(g, textRect);
                this.ensureSize(clientHeight, textRect.Size);
            }


            /// <summary>
            /// Draws the message
            /// </summary>
            private void drawMessage(Graphics g, Rectangle rect)
            {
                if (string.IsNullOrEmpty(this.Message))
                    return;

                g.SetHighQuality();

                this.sgMessage.LineAlignment = this.sgNote.IsEmpty ? StringAlignment.Center : StringAlignment.Near;
                this.sgMessage.Draw(g, Brushes.White, rect);

                if (!this.sgNote.IsEmpty)
                {
                    var m = this.sgMessage.Measure(rect.Width).Height;
                    rect.Y += m;
                    rect.Height -= m;

                    this.sgNote.Draw(g, Brushes.White, rect);
                }
            }


            /// <summary>
            /// Makes sure the size of the balloon matches the contents
            /// </summary>
            private void ensureSize(int clientHeight, Size size)
            {
                var textHeight = this.sgMessage.Measure(size.Width).Height + this.sgNote.Measure(size.Width).Height;
                var dstHeight = textHeight <= size.Height || size.Height < 0
                    ? clientHeight
                    : Math.Min(clientHeight + textHeight - size.Height, Screen.PrimaryScreen.Bounds.Height / 3);

                if (this.Height != dstHeight)
                {
                    if (dstHeight > this.Height)
                        this.Location = new Point(this.Location.X, this.Location.Y - (dstHeight - this.Height));
                    this.Height = dstHeight;
                }
            }


            /// <summary>
            /// Draws the icon
            /// </summary>
            private Size drawIcon(Graphics g, Rectangle rect)
            {
                if (this.Icon == null)
                    return Size.Empty;

                rect = new Rectangle(rect.X, rect.Y, this.Icon.Width * rect.Height / this.Icon.Height, rect.Height);
                g.DrawImage(this.Icon, rect);
                return rect.Size;
            }

            #endregion
        }

        #endregion

        private static BalloonControl tooltip = new BalloonControl();
        private static readonly Timer timer = new Timer();
        

        static BalloonTooltip()
        {
            timer.Tick += onTimerTick;
            tooltip.MouseDown += (s, e) => Hide();
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
            tooltip.ShowInCorner();
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
            
            tooltip.InvokeIfRequired(() =>
            {
                resetTimer(timeout);

                tooltip.LostFocus -= onLostFocus;
                tooltip.Icon = icon;
                tooltip.Message = message;
                tooltip.Note = note;
                tooltip.ShowInCorner();

                if (timeout == 0)
                {
                    tooltip.Focus();
                    tooltip.LostFocus += onLostFocus;
                }
            });
        }

        /// <summary>
        /// Handles the focus lost event
        /// </summary>
        private static void onLostFocus(object sender, EventArgs e)
        {
            if (!timer.Enabled)
                Hide();
        }


        /// <summary>
        /// Hides the notification
        /// </summary>
        public static void Hide()
        {
            tooltip.InvokeIfRequired(() =>
            {
                resetTimer();
                tooltip.LostFocus -= onLostFocus;
                tooltip.Hide();
            });
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
