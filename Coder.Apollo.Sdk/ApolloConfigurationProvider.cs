using Coder.Apollo.Sdk.DataSource;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coder.Apollo.Sdk
{
    public class ApolloConfigurationProvider : ConfigurationProvider
    {
        private readonly RemoteConfigs _remoteConfig;
        private static ILogger Logger => ApolloConfigurationLogger.Logger;

        public ApolloConfigurationProvider(ApolloOptions apolloOptions)
        {
            //由于服务端会hold住请求60秒，所以请确保客户端访问服务端的超时时间要大于60秒。->摘抄自官方文档
            _remoteConfig = RemoteConfigs.GetInstance(apolloOptions);
        }

        public override void Load()
        {
            GetConfigData();//对于第一次初始化配置时，需要单独调用，后续的走定时线程拉取
            Task.Factory.StartNew(() =>
            {
                // 定时轮询远程的配置数据
                while (true)
                {
                    Thread.Sleep(30000);//30秒定时轮训远程接口（视实时性要求而定，对于平台足够了）
                    GetConfigData();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void GetConfigData()
        {
            try
            {
                var configNotification = _remoteConfig.GetConfigNotification();
                var namespaceNames = configNotification.Select(m => m.NamespaceName).ToArray();
                var configs = _remoteConfig.GetConfigsByNamespaces(namespaceNames, out var hasChanged);
                //如果配置有变更则更新配置到内存和本地文件
                if (hasChanged)
                {
                    var configsArray = configs.SelectMany(m => m.Configurations).ToArray();
                    var newConfigData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    Array.ForEach(configsArray, m => { newConfigData[m.Key] = m.Value; });
                    Data = newConfigData;
                    LocalConfigs.Set(base.Data);
                    OnReload();
                }
                EnsureExistsLocalConfigFile();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Apollo定时轮询核心方法异常");
                EnsureExistsLocalConfigFile();
                GetConfigFromLocalConfigFile();
            }
        }

        /// <summary>
        /// 确保本地配置文件存在
        /// </summary>
        private void EnsureExistsLocalConfigFile()
        {
            if (!LocalConfigs.ExistsCacheFile())
            {
                Logger?.LogWarning("Apollo本地缓存配置文件不存在，即将创建");
                LocalConfigs.Set(base.Data);
            }
        }

        /// <summary>
        /// 读取本地缓存配置，用于Apollo挂掉情况
        /// </summary>
        private void GetConfigFromLocalConfigFile()
        {
            var configs = LocalConfigs.Get();
            Data = configs;
            OnReload();
            Logger?.LogCritical("Apollo拉取远程配置失败，当前已命中本地缓存配置文件。请及时联系运维人员，修复Apollo");
        }
    }
}