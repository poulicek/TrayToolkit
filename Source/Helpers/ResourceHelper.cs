using System.Drawing;
using System.IO;
using System.Reflection;

namespace TrayToolkit.Helpers
{
    public static class ResourceHelper
    {
        private static Assembly assembly = Assembly.GetEntryAssembly();

        /// <summary>
        /// Returns the embedded image instance
        /// </summary>
        public static Bitmap GetResourceImage(string imagePath)
        {
            using (var s = GetResourceStream(imagePath))
                return s == null ? null : new Bitmap(s);
        }


        /// <summary>
        /// Returns the embedded resource stream
        /// </summary>
        public static Stream GetResourceStream(string path)
        {
            return assembly.GetManifestResourceStream($"{assembly.EntryPoint.DeclaringType.Namespace.Split('.')[0]}.{path}");
        }
    }
}
