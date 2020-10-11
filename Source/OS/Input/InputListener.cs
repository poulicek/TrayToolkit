using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.OS.Input
{
    public class InputListener : IDisposable
    {
        private Keys lastKeyDown;

        private IntPtr mouseHookId = IntPtr.Zero;
        private IntPtr keyboardHookId = IntPtr.Zero;

        private ActionKey[] actionKeys;
        private User32.LowLevelCallbackProc mouseCallback;
        private User32.LowLevelCallbackProc keyboardCallback;

        public event Action KeyBlocked;
        public event Action<Keys> KeyPressed;
        public event Action<Keys> KeyReleased;

        public bool BlockInput { get; private set; }


        /// <summary>
        /// Sets the hooks for keyboard and mouse (depending on if input blocking is requested)
        /// </summary>
        public void Listen(bool blockInput, params ActionKey[] actionKeys)
        {
            this.Stop();

            this.BlockInput = blockInput;
            this.actionKeys = actionKeys;
            var hModule = Marshal.GetHINSTANCE(Assembly.GetEntryAssembly().GetModules()[0]);

            // callbacks have their instance variables to prevent their destrcution by garbage collector
            this.keyboardHookId = User32.SetWindowsHookEx(User32.WH_KEYBOARD_LL, this.keyboardCallback = new User32.LowLevelCallbackProc(this.keyboardHookCallback), hModule, 0);            
            if (blockInput)
                this.mouseHookId = User32.SetWindowsHookEx(User32.WH_MOUSE_LL, this.mouseCallback = new User32.LowLevelCallbackProc(this.mouseBlockingHookCallback), hModule, 0);
        }


        /// <summary>
        /// Clears the hooks
        /// </summary>
        public void Stop()
        {
            if (this.mouseHookId != IntPtr.Zero)
                User32.UnhookWindowsHookEx(this.mouseHookId);

            if (this.keyboardHookId != IntPtr.Zero)
                User32.UnhookWindowsHookEx(this.keyboardHookId);

            this.mouseHookId = IntPtr.Zero;
            this.keyboardHookId = IntPtr.Zero;

            this.mouseCallback = null;
            this.keyboardCallback = null;
            this.BlockInput = false;
        }

        #region Callbacks

        /// <summary>
        /// Process the mouse events and always blocks them
        /// </summary>
        private IntPtr mouseBlockingHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (wParam == (IntPtr)User32.MouseMessages.WM_LBUTTONDOWN || wParam == (IntPtr)User32.MouseMessages.WM_RBUTTONDOWN)
                    this.KeyBlocked?.Invoke();                
            }
            catch { }

            return new IntPtr(-1);
        }


        /// <summary>
        /// Processes the keyboard events and blocks them if blocking is on
        /// </summary>
        private IntPtr keyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var key = this.getKey(lParam);
            var isDown = (int)wParam == User32.WM_KEYDOWN || (int)wParam == User32.WM_SYSKEYDOWN;

            try
            {
                // ingore repeating messages
                if (this.lastKeyDown != key || !isDown)
                {
                    // finding the special key
                    if (this.tryInvokeActionKey(key, isDown))
                        return new IntPtr(-1);

                    // invoking key pressed event
                    if (isDown)
                        this.KeyPressed?.Invoke(key);
                    else
                        this.KeyReleased?.Invoke(key);
                }

                // propagate event if input is not blocked or is a modifier
                if (!this.BlockInput || this.isKeyModifier(key))
                    return User32.CallNextHookEx(this.keyboardHookId, nCode, wParam, lParam);

                // inform about the blocked key
                if (isDown && this.lastKeyDown != key)
                    this.KeyBlocked?.Invoke();
            }
            catch { }
            finally
            {
                this.lastKeyDown = isDown ? key : Keys.None;
            }

            // blocking the input;
            return new IntPtr(-1);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns true of the key is pressed
        /// </summary>
        private bool isKeyPressed(Keys key)
        {
            return (User32.GetKeyState((int)key) & 0x80) == 0x80;
        }


        /// <summary>
        /// Converts the pointer into structure
        /// </summary>
        private Keys getKey(IntPtr ptr)
        {
            var key = (Keys)((User32.KeyboardHookStruct)Marshal.PtrToStructure(ptr, typeof(User32.KeyboardHookStruct))).VirtualKeyCode;
            var res = key;

            if (isKeyModifier(key))
                res = Keys.None;

            if (isKeyPressed(Keys.ControlKey) || key == Keys.LControlKey || key == Keys.RControlKey)
                res |= Keys.Control;

            if (isKeyPressed(Keys.ShiftKey) || key == Keys.LShiftKey || key == Keys.RShiftKey)
                res |= Keys.Shift;

            if (isKeyPressed(Keys.Menu) || key == Keys.LMenu || key == Keys.RMenu)
                res |= Keys.Alt;

            return res;
        }


        /// <summary>
        /// Checks if the key combination was triggered
        /// </summary>
        private bool tryInvokeActionKey(Keys key, bool isDown)
        {
            foreach (var k in actionKeys)
                if (k.IsMatch(key) && k.SetPressedState(isDown) == true)
                    return true;

            return false;
        }


        /// <summary>
        /// Returns true if the key is a modifier
        /// </summary>
        private bool isKeyModifier(Keys key)
        {
            return key >= Keys.LShiftKey && key <= Keys.RMenu;
        }

        public void Dispose()
        {
            this.Stop();
        }

        #endregion
    }
}
