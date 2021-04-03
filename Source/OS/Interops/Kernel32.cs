using System.Runtime.InteropServices;

namespace TrayToolkit.OS.Interops
{
    public static class Kernel32
    {
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();
    }
}
