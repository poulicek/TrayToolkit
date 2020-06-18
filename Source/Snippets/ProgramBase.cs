using System;
using System.Reflection;
using System.Threading;

namespace TrayToolkit.Snippets
{
    static class Program
    {
        private static readonly Mutex mutex = new Mutex(false, Assembly.GetExecutingAssembly().GetName().Name);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!mutex.WaitOne(0, false))
                return;

            AppDomain.CurrentDomain.UnhandledException += onUnhandledException;
        }
        


        private static void onUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw e.ExceptionObject as Exception;
        }
    }
}
