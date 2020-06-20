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
        public static Bitmap GetResourceImage(string imagePath, Assembly customAssembly = null)
        {
            using (var s = GetResourceStream(imagePath, customAssembly))
                return s == null ? null : new Bitmap(s);
        }


        /// <summary>
        /// Returns the embedded resource stream
        /// </summary>
        public static Stream GetResourceStream(string path, Assembly customAssembly = null)
        {
            var assm = customAssembly ?? assembly;
            var nameSpace = assm.EntryPoint?.DeclaringType.Namespace.Split('.')[0] ?? assm.GetName().Name;
            return assm.GetManifestResourceStream($"{nameSpace}.{path}");
        }
    }
}
