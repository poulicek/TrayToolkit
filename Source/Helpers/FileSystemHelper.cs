using System.IO;
using System.Reflection;

namespace TrayToolkit.Helpers
{
    public static class FileSystemHelper
    {
        private static Assembly assembly = Assembly.GetEntryAssembly();

        public static string AppFolder { get { return Path.GetDirectoryName(assembly.CodeBase); } }
    }
}
