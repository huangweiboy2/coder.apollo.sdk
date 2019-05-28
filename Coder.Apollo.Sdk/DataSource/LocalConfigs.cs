using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Coder.Apollo.Sdk.DataSource
{
    public static class LocalConfigs
    {
        private static readonly string ConfigCacheFullName = Path.Combine(Environment.CurrentDirectory, "appsettings.cache.json");

        private static readonly string LockObj = string.Empty;

        internal static void Set(IDictionary<string, string> data)
        {
            lock (LockObj)
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(ConfigCacheFullName, json, Encoding.UTF8);
            }
        }

        internal static bool HasCacheFile()
        {
            return File.Exists(ConfigCacheFullName);
        }

        public static IDictionary<string, string> Get()
        {
            var txt = File.ReadAllText(ConfigCacheFullName, Encoding.UTF8);
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(txt);
            return result;
        }
    }
}