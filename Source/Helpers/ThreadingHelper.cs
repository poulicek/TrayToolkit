using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using TrayToolkit.UI;

namespace TrayToolkit.Helpers
{
    public static class ThreadingHelper
    {
        public static event Action<Exception> UnhandledException;

        /// <summary>
        /// Invokes the action in a new thread
        /// </summary>
        public static Thread DoAsync(Action callback)
        {
            var t = new Thread(() =>
            {
                try
                {
                    callback?.Invoke();
                }
                catch (Exception ex) { UnhandledException?.Invoke(ex); }
            })
            { IsBackground = true };
            t.Start();
            return t;
        }


        /// <summary>
        /// Invokes the action in a new thread and waits till its finished for given period.
        /// If not finished in time, the tread gets aborted.
        /// </summary>
        public static void DoAsync(Action callback, int waitMaxMs)
        {
            try
            {
                var t = DoAsync(callback);

                if (waitMaxMs > 0)
                {
                    Thread.Sleep(waitMaxMs);
                    t.Abort();
                }
            }
            catch { }
        }


        /// <summary>
        /// Invokes the action if it is from different thread
        /// </summary>
        public static void InvokeIfRequired(this Control ctrl, Action callback)
        {
            if (ctrl.InvokeRequired)
                ctrl.BeginInvoke(callback);
            else
                callback();
        }


        /// <summary>
        /// Handles the exception
        /// </summary>
        public static void HandleException(Exception ex, bool debugOnly = true)
        {

#if !DEBUG
            if (debugOnly)
                return;
#endif
            BalloonTooltip.Show(Assembly.GetEntryAssembly().GetName().Name + " - Error", null, ex.ToString(), 5000);

        }
    }
}
