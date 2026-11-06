using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace TrayToolkit.Helpers
{
    public static class IniHelper
    {
        /// <summary>
        /// Reads the INI file
        /// </summary>
        public static NameValueCollection ReadFile(string path)
        {
            return File.Exists(path) ? Parse(File.ReadAllText(path)) : new NameValueCollection();
        }


        /// <summary>
        /// Writes the INI file
        /// </summary>
        public static void WriteFile(string path, NameValueCollection ini)
        {
            File.WriteAllText(path, Serialize(ini));
        }


        /// <summary>
        /// Parses the string
        /// </summary>
        public static NameValueCollection Parse(string ini)
        {
            // a single line is treated as a query string (key=value&key=value), otherwise each line is a pair
            var pairs = ini.Replace("\r", null).Split(ini.Contains("\n") ? '\n' : '&');
            var normalized = new NameValueCollection();

            foreach (var pair in pairs)
            {
                // lines without the key are skipped
                var index = pair.IndexOf('=');
                if (index < 0)
                    continue;

                var k = decode(pair.Substring(0, index)).Trim();
                var v = decode(pair.Substring(index + 1)).Trim().Trim(new char[] { '"', ',' });

                // skipping the section names and comments
                if (string.IsNullOrEmpty(k) || string.IsNullOrEmpty(v) || k.StartsWith(";") || k.StartsWith("["))
                    continue;

                normalized[k] = v;
            }

            return normalized;
        }


        /// <summary>
        /// Decodes the URL-encoded text the same way the query string parser does
        /// </summary>
        private static string decode(string text)
        {
            return Uri.UnescapeDataString(text.Replace('+', ' '));
        }


        /// <summary>
        /// Serializes the data collection
        /// </summary>
        public static string Serialize(NameValueCollection ini)
        {
            var sb = new StringBuilder();
            foreach (var key in ini.AllKeys)
                sb.AppendFormat("{0} = {1}{2}", key, ini[key], Environment.NewLine);
            return sb.ToString();
        }
    }
}
