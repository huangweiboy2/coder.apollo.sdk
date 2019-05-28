using Coder.Apollo.Sdk.DataSource;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coder.Apollo.Sdk
{
    public class ApolloConfigurationProvider : ConfigurationProvider
    {
        private readonly RemoteConfigs _remoteConfig;

        public ApolloConfigurationProvider(ApolloOptions apolloOptions)
        {
            //由于服务端会hold住请求60秒，所以请确保客户端访问服务端的超时时间要大于60秒。->摘抄自官方文档
            _remoteConfig = RemoteConfigs.GetInstance(apolloOptions, 65);
        }

        public override void Load()
        {
            GetConfigDta();
            // 定时轮询远程的配置数据
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(30000);
                    GetConfigDta();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void GetConfigDta()
        {
            try
            {
                var configNotification = _remoteConfig.GetConfigNotification();
                if (configNotification.Exception != null)
                {
                    //todo 日志记录
                    GetLocalConfig();
                    return;
                }
                var namespaceNames = configNotification.Data.Select(m => m.NamespaceName).ToArray();
                var configs = _remoteConfig.GetConfigs(namespaceNames);
                if (configs.Exception != null)
                {
                    //todo 日志记录
                    GetLocalConfig();
                    return;
                }
                var configsArray = configs.Data.SelectMany(m => m.Configurations).ToArray();
                //如果配置有变更则更新配置到内存和本地文件
                if (configs.HasChanged)
                {
                    Array.ForEach(configsArray, m => { Data[m.Key] = m.Value; });
                    LocalConfigs.Set(base.Data);
                    OnReload();
                }
                //如果本地缓存文件被删除，则强制再创建
                if (!LocalConfigs.HasCacheFile())
                {
                    LocalConfigs.Set(base.Data);
                }
            }
            catch (Exception ex)
            {
                //todo 日志记录
                GetLocalConfig();
            }
        }

        /// <summary>
        /// 获取本地配置
        /// </summary>
        private void GetLocalConfig()
        {
            if (!LocalConfigs.HasCacheFile())
            {
                LocalConfigs.Set(base.Data);
            }
            else
            {
                try
                {
                    //防止在使用过程中，有人调整本地配置缓存文件
                    var configs = LocalConfigs.Get();
                    foreach (var config in configs)
                    {
                        Data[config.Key] = config.Value;
                    }
                    OnReload();
                }
                catch //(Exception e)
                {
                    //todo 日志记录
                }
            }
        }
    }
}