using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;

namespace TrayToolkit.Helpers
{
    public static class AppConfigHelper
    {
        private static string configPath = Path.Combine(FileSystemHelper.AppFolder, Assembly.GetEntryAssembly().GetName().Name + ".ini");
        private static NameValueCollection cfg;


        public static string Get(string key)
        {
            if (cfg == null)
                load();
            return cfg?[key];
        }


        public static T Get<T>(string key)
            where T : struct
        {
            return Get<T>(key, default(T));
        }


        public static T Get<T>(string key, T defaultValue)
            where T: struct
        {
            var value = Get(key);

            if (typeof(T).IsEnum)
                return Enum.TryParse<T>(value?.ToString(), out T result) ? result : defaultValue;

            return value == null
                ? defaultValue
                : (T)Convert.ChangeType(value, typeof(T));
        }


        public static void Set(string key, object value = null)
        {
            if (cfg == null)
                cfg = new NameValueCollection();

            if (value == null)
                cfg.Remove(key);
            else
                cfg[key] = value.ToString();

            save();
        }


        private static void load()
        {
            cfg = IniHelper.ReadFile(configPath);
        }

        private static void save()
        {
            IniHelper.WriteFile(configPath, cfg);
        }
    }
}
