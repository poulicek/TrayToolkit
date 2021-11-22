using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrayToolkit.OS.Interops;

namespace TrayToolkit.Helpers
{
    public static class FileSystemHelper
    {
        private static Assembly assembly = Assembly.GetEntryAssembly();

        /// <summary>
        /// Returns the main application folder
        /// </summary>
        public static string AppFolder { get { return Path.GetDirectoryName(assembly.CodeBase.Replace("file:", null).TrimStart('/')); } }


        /// <summary>
        /// Returns the user-chosen folder
        /// </summary>
        public static string GetFolder()
        {
            using (var dlg = new FolderBrowserDialog())
                if (dlg.ShowDialog() == DialogResult.OK)
                    return dlg.SelectedPath;

            return null;
        }


        /// <summary>
        /// Finds all files with the given extensions
        /// </summary>
        public static List<string> FindFiles(string folder, params string[] filters)
        {
            var files = new List<string>();
            Parallel.ForEach(filters, (filter) => files.AddRange(Directory.GetFiles(folder, filter, SearchOption.AllDirectories)));
            return files;
        }


        /// <summary>
        /// Returns true if the files match
        /// </summary>
        public static bool Matches(this FileInfo srcFile, FileInfo dstFile, int noOfComparedBytes = 5)
        {
            if (srcFile.Length != dstFile.Length)
                return false;

            var length = (int)srcFile.Length;
            var step = noOfComparedBytes == 0 ? 1 : length / noOfComparedBytes;

            using (var srcStream = srcFile.OpenRead())
            using (var dstStream = dstFile.OpenRead())
            {
                for (int i = 0; i < length; i += step)
                {
                    srcStream.Seek(i, SeekOrigin.Begin);
                    dstStream.Seek(i, SeekOrigin.Begin);

                    if (srcStream.ReadByte() != dstStream.ReadByte())
                        return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Returns the size of the file on disk
        /// </summary>
        public static long GetDiskSize(this FileInfo file)
        {
            var losize = Kernel32.GetCompressedFileSizeW(file.FullName, out var hosize);
            return (long)hosize << 32 | losize;
        }
    }
}
