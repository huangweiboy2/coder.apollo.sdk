using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Coder.Apollo.Sdk.DataSource
{
    internal static class LocalConfigs
    {
        private static readonly string ConfigCacheFullName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.apollo.cache.json");

        private static readonly string LockObj = string.Empty;

        /// <summary>
        /// 将配置设置到本地缓存文件中
        /// </summary>
        /// <param name="data"></param>
        internal static void Set(IDictionary<string, string> data)
        {
            lock (LockObj)
            {
                var orderDic = data.OrderBy(m => m.Key).ToDictionary(m => m.Key, m => m.Value);
                var json = JsonConvert.SerializeObject(orderDic, Formatting.Indented);
                File.WriteAllText(ConfigCacheFullName, json, Encoding.UTF8);
            }
        }

        /// <summary>
        /// 是否存在缓存文件
        /// </summary>
        /// <returns></returns>
        internal static bool ExistsCacheFile()
        {
            return File.Exists(ConfigCacheFullName);
        }

        /// <summary>
        /// 获取本地缓存文件中的配置
        /// </summary>
        /// <returns></returns>
        internal static IDictionary<string, string> Get()
        {
            var txt = File.ReadAllText(ConfigCacheFullName, Encoding.UTF8);
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(txt);
            return result;
        }
    }
}