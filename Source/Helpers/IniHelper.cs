using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

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
            // converting INI form to query string
            var query = (ini.Contains("\n") ? ini.Replace("&", "%26") : ini).Replace("\r", null).Replace("\n", "&");
            var parsed = HttpUtility.ParseQueryString(query);
            var normalized = new NameValueCollection();

            foreach (string key in parsed.Keys)
            {
                var k = key?.Trim();
                var v = parsed[key]?.Trim().Trim(new char[] { '"', ',' });

                // skipping the section names and comments
                if (string.IsNullOrEmpty(k) || string.IsNullOrEmpty(v) || k.StartsWith(";") || k.StartsWith("["))
                    continue;

                normalized[k] = v;
            }

            return normalized;
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
