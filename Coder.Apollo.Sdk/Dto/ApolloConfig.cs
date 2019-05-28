using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Coder.Apollo.Sdk.Dto
{
    public class ApolloConfig
    {
        /// <summary>
        /// 应用Id
        /// </summary>
        [JsonProperty("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// 集群名 默认default
        /// </summary>
        [JsonProperty("cluster")]
        public string Cluster { get; set; }

        /// <summary>
        /// Namespace的名字
        /// </summary>
        [JsonProperty("namespaceName")]
        public string NamespaceName { get; set; }

        /// <summary>
        /// 配置的键值信息
        /// </summary>
        [JsonProperty("configurations")]
        public IDictionary<string, string> Configurations { get; set; }

        /// <summary>
        /// 上一次的releaseKey 默认-1
        /// </summary>
        [JsonProperty("releaseKey")]
        public string ReleaseKey { get; set; }

        #region 静态方法

        private static readonly ConcurrentDictionary<string, ApolloConfig> Caches = new ConcurrentDictionary<string, ApolloConfig>(StringComparer.OrdinalIgnoreCase);

        internal static ApolloConfig GetByNamespace(string namespaceName)
        {
            namespaceName = namespaceName.Trim();
            Caches.TryGetValue(namespaceName, out var model);
            return model;
        }

        internal static void AddOrUpdate(ApolloConfig apolloConfig)
        {
            var namespaceName = apolloConfig.NamespaceName.Trim();
            Caches.AddOrUpdate(namespaceName, apolloConfig, (k, v) => v);
        }

        #endregion 静态方法
    }
}