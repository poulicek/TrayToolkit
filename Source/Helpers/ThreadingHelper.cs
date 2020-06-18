using System;
using System.Threading;

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
            });
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
    }
}
