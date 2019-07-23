using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Coder.Apollo.Sdk.Dto;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Coder.Apollo.Sdk.DataSource
{
    //Apollo 接入指南链接如下：
    // https://github.com/ctripcorp/apollo/wiki/%E5%85%B6%E5%AE%83%E8%AF%AD%E8%A8%80%E5%AE%A2%E6%88%B7%E7%AB%AF%E6%8E%A5%E5%85%A5%E6%8C%87%E5%8D%97
    internal sealed class RemoteConfigs
    {
        private readonly ApolloOptions _apolloOptions;
        private static HttpClient _httpClient;
        private static ILogger Logger => ApolloConfigurationLogger.Logger;

        #region 构造函数

        private RemoteConfigs()
        {
        }

        private RemoteConfigs(ApolloOptions apolloOptions)
        {
            _apolloOptions = apolloOptions;
            if (_httpClient == null)
            {
                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri(_apolloOptions.MetaServer.Trim()),
                    //由于服务端会hold住请求60秒，所以请确保客户端访问服务端的超时时间要大于60秒。->摘抄自官方文档
                    Timeout = TimeSpan.FromSeconds(65)
                };
            }
        }

        #endregion 构造函数

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="apolloOptions"></param>
        /// <returns></returns>
        internal static RemoteConfigs GetInstance(ApolloOptions apolloOptions)
        {
            var model = new RemoteConfigs(apolloOptions);
            return model;
        }

        /// <summary>
        /// 获取所有的通知对象
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<ApolloConfigNotification> GetConfigNotification()
        {
            var sbLog = new StringBuilder("Apollo获取命名空间变更信息");
            var result = new List<ApolloConfigNotification>();
            var requestUrl = _apolloOptions.GetNamespaceNotificationsUrl();
            sbLog.AppendLine($"访问地址：{requestUrl}");
            try
            {
                var response = _httpClient.GetAsync(requestUrl).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var jsonStr = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                sbLog.AppendLine($"返回结果：{jsonStr}");
                var jsonModel = JsonConvert.DeserializeObject<List<ApolloConfigNotification>>(jsonStr);
                result = jsonModel;
                Logger?.LogDebug(sbLog.ToString());
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, sbLog.ToString());
                throw;
            }
            return result;
        }

        /// <summary>
        /// 获取命名空间下的所有配置
        /// </summary>
        /// <param name="namespaceNames">命名空间数组</param>
        /// <param name="hasChangedData">是否存在变的配置</param>
        /// <returns></returns>
        internal IEnumerable<ApolloConfig> GetConfigsByNamespaces(IEnumerable<string> namespaceNames, out bool hasChangedData)
        {
            var sbLog = new StringBuilder("Apollo获取命名空间下的所有配置");
            sbLog.AppendLine($"请求的命名空间集合：{string.Join(",", namespaceNames)}");
            hasChangedData = false;
            var result = new List<ApolloConfig>();
            try
            {
                foreach (var namespaceName in namespaceNames.Select(m => m.Trim()).Distinct())
                {
                    var oldModel = ApolloConfig.GetByNamespace(namespaceName);
                    var releaseKey = string.IsNullOrWhiteSpace(oldModel?.ReleaseKey) ? "-1" : oldModel.ReleaseKey;
                    var requestUrl = _apolloOptions.GetConfigsByNamespacesUrl(namespaceName, releaseKey);
                    sbLog.AppendLine($"请求{namespaceName}命名空间，访问地址：{requestUrl}");
                    var response = _httpClient.GetAsync(requestUrl).GetAwaiter().GetResult();
                    if (response.StatusCode == HttpStatusCode.NotModified) //返回304表示服务端没有变更
                    {
                        sbLog.AppendLine($"请求{namespaceName}命名空间，服务端无配置变更");
                        result.Add(oldModel);
                    }
                    else
                    {
                        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        sbLog.AppendLine($"请求{namespaceName}命名空间，服务端配置有变更，最新配置为：{json}");
                        var newModel = JsonConvert.DeserializeObject<ApolloConfig>(json);
                        ApolloConfig.AddOrUpdate(newModel);
                        result.Add(newModel);
                        if (newModel.ReleaseKey != releaseKey)
                        {
                            hasChangedData = true;
                        }
                    }
                    Logger?.LogDebug(sbLog.ToString());
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, sbLog.ToString());
                throw;
            }
            return result;
        }
    }
}