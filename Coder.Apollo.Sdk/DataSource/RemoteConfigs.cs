using Coder.Apollo.Sdk.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Coder.Apollo.Sdk.DataSource
{
    internal sealed class RemoteConfigs
    {
        private static readonly ConcurrentDictionary<int, HttpClient> HttpClientDictionary = new ConcurrentDictionary<int, HttpClient>();
        private readonly int _requestTimeoutSecond;
        private readonly ApolloOptions _apolloOptions;

        #region 构造函数

        private RemoteConfigs()
        {
        }

        private RemoteConfigs(ApolloOptions apolloOptions, int requestTimeoutSecond)
        {
            _requestTimeoutSecond = requestTimeoutSecond;
            _apolloOptions = apolloOptions;
            HttpClientDictionary.TryGetValue(requestTimeoutSecond, out var httpClient);
            if (httpClient == null)
            {
                httpClient = new HttpClient
                {
                    BaseAddress = new Uri(_apolloOptions.MetaServer.Trim()),
                    Timeout = TimeSpan.FromSeconds(requestTimeoutSecond)
                };
                HttpClientDictionary.TryAdd(requestTimeoutSecond, httpClient);
            }
        }

        #endregion 构造函数

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="apolloOptions"></param>
        /// <param name="requestTimeoutSecond"></param>
        /// <returns></returns>
        internal static RemoteConfigs GetInstance(ApolloOptions apolloOptions, int requestTimeoutSecond)
        {
            var model = new RemoteConfigs(apolloOptions, requestTimeoutSecond);
            return model;
        }

        /// <summary>
        /// 获取所有的通知对象
        /// </summary>
        /// <returns></returns>
        internal ApolloConfigNotificationOutput GetConfigNotification()
        {
            var result = new ApolloConfigNotificationOutput(null, true, null);
            try
            {
                var httpClient = HttpClientDictionary[_requestTimeoutSecond];
                var response = httpClient.GetAsync(_apolloOptions.GetNotificationsUrl()).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                var jsonStr = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var jsonModel = JsonConvert.DeserializeObject<ICollection<ApolloConfigNotification>>(jsonStr);
                result.HasChanged = true;
                result.Data = jsonModel;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        /// <summary>
        /// 获取命名空间下的所有配置
        /// </summary>
        /// <param name="namespaceNames">命名空间数组</param>
        /// <returns></returns>
        internal ApolloConfigOutput GetConfigs(ICollection<string> namespaceNames)
        {
            var result = new ApolloConfigOutput(null, true, null);
            try
            {
                var models = new List<ApolloConfig>();
                var httpClient = HttpClientDictionary[_requestTimeoutSecond];
                var changedFlagKv = new KeyValuePair<bool, int>(false, 0);//key 表示是否已经变更，value表示是否锁定变量 0未锁定 1锁定
                foreach (var namespaceName in namespaceNames.Select(m => m.Trim()).Distinct())
                {
                    var oldModel = ApolloConfig.GetByNamespace(namespaceName);
                    var releaseKey = string.IsNullOrWhiteSpace(oldModel?.ReleaseKey) ? "-1" : oldModel.ReleaseKey;
                    var urlPath = _apolloOptions.GetConfigsUrl(namespaceName, releaseKey);
                    var response = httpClient.GetAsync(urlPath).GetAwaiter().GetResult();
                    if (response.StatusCode == HttpStatusCode.NotModified) //返回304表示服务端没有变更
                    {
                        models.Add(oldModel);
                    }
                    else
                    {
                        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        var newModel = JsonConvert.DeserializeObject<ApolloConfig>(json);
                        ApolloConfig.AddOrUpdate(newModel);
                        models.Add(newModel);
                        if (newModel.ReleaseKey != releaseKey && changedFlagKv.Value == 0)
                        {
                            changedFlagKv = new KeyValuePair<bool, int>(true, 1);
                        }
                    }
                    result.HasChanged = changedFlagKv.Key;
                    result.Data = models.ToArray();
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }
    }
}