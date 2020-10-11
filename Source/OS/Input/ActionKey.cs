using System;
using System.Windows.Forms;
using TrayToolkit.Helpers;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.OS.Input
{
    public class ActionKey
    {
        private const int LONG_PRESS_TIMEOUT = 600;

        private readonly Timer longPressTimer = new Timer() { Interval = LONG_PRESS_TIMEOUT };

        public static implicit operator ActionKey(Keys k) => new ActionKey(k);

        public event Func<bool> Pressed;
        public event Func<bool> Released;
        public event Action LongPress;

        public Keys Key { get; set; }

        public bool IsPressed { get; private set; }
        public bool WasLongPressed { get; private set; }


        public ActionKey(Keys key)
        {
            this.Key = key.IsNumeric() ? key.ToSmallNumeric() : key;
            this.longPressTimer.Tick += this.longPressCallback;
        }


        /// <summary>
        /// Presses the key
        /// </summary>
        public void Press()
        {
            this.Key.Down();
        }


        /// <summary>
        /// Returns true if the action key matches the key
        /// </summary>
        public bool IsMatch(Keys key)
        {
            return this.Key == key || (this.Key.IsNumeric() && this.Key == key.ToSmallNumeric());
        }


        /// <summary>
        /// Sets the pressed state
        /// </summary>
        public bool SetPressedState(bool pressed)
        {
            if (this.IsPressed == pressed)
                return false;

            this.IsPressed = false;
            this.longPressTimer.Stop();

            // key release
            if (pressed)
            {
                this.IsPressed = true;
                this.WasLongPressed = false;
                this.longPressTimer.Start();
                
                return this.Pressed?.Invoke() == true;
            }

            return this.Released?.Invoke() == true;
        }


        /// <summary>
        /// Long press detection
        /// </summary>
        private void longPressCallback(object sender, EventArgs e)
        {
            this.longPressTimer.Stop();
            this.WasLongPressed = this.IsPressed;
            this.LongPress?.Invoke();
        }


        private string keyToString(Keys key)
        {
            if (key >= Keys.D0 && key <= Keys.D9)
                return key.ToString().Substring(1);

            return key.ToString();
        }

        public override string ToString()
        {
            var str = string.Empty;
            if (this.Key.HasFlag(Keys.Control))
                str += "Ctrl,";

            if (this.Key.HasFlag(Keys.Alt))
                str += "Alt,";

            if (this.Key.HasFlag(Keys.Shift))
                str += "Shift,";

            var rootKey = this.Key.GetUnmodifiedKey();
            if (rootKey != Keys.None)
                str += this.keyToString(rootKey);

            return str.Trim(',').Replace(',', '+');
        }
    }
}
