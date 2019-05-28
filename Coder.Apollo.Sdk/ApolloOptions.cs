using Coder.Apollo.Sdk.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Coder.Apollo.Sdk
{
    public class ApolloOptions
    {
        /// <summary>
        /// 应用Id
        /// </summary>s
        public string AppId { get; set; }

        /// <summary>
        /// 服务发现Eureka地址
        /// </summary>
        public string MetaServer { get; set; }

        /// <summary>
        /// 集群名	默认：default
        /// </summary>
        public string Cluster { get; set; } = "default";

        /// <summary>
        /// 应用部署的机器ip 用来实现灰度发布,默认为空字符
        /// </summary>
        public string DeployIp { get; set; } = string.Empty;

        /// <summary>
        /// 命名空间集合
        /// </summary>
        public IList<string> Namespaces { get; set; } = new List<string>();

        #region 获取Apoll Api地址

        /// <summary>
        /// 获取通知的Url地址
        /// </summary>
        /// <returns></returns>
        internal string GetNotificationsUrl()
        {
            Validate();
            var namespaces = Namespaces.Distinct().OrderBy(m => m).ToArray();
            var list = new List<ApolloConfigNotification>();
            foreach (var item in namespaces)
            {
                list.Add(new ApolloConfigNotification { NamespaceName = item, NotificationId = -1 });
            }
            var monitorNamespaces = JsonConvert.SerializeObject(list, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var url = $"{MetaServer}/notifications/v2?appId={AppId}&cluster={Cluster}&notifications={WebUtility.UrlEncode(monitorNamespaces)}";
            return url;
        }

        ///  <summary>
        /// 根据命名空间和ReleaseKey获取对应的配置Url地址
        ///  </summary>
        ///  <param name="namespaceName"></param>
        ///  <param name="releaseKey"></param>
        ///  <returns></returns>
        internal string GetConfigsUrl(string namespaceName, string releaseKey)
        {
            Validate();
            var url = $"{MetaServer}/configs/{AppId}/{Cluster}/{namespaceName}?releaseKey={releaseKey.Trim()}&ip={DeployIp}";
            return url;
        }

        #endregion 获取Apoll Api地址

        #region 私有方法

        /// <summary>
        /// 验证实体options的数据有效性
        /// </summary>
        private void Validate()
        {
            this.AppId = AppId.Trim();
            this.MetaServer = MetaServer.Trim().TrimEnd('/');
            this.Cluster = Cluster.Trim();
            this.DeployIp = string.IsNullOrWhiteSpace(DeployIp) ? GetHostIp() : DeployIp.Trim();
            this.Namespaces = Namespaces.Select(m => m.Trim()).Distinct().ToArray();
        }

        private static volatile string _hostIp = string.Empty;

        /// <summary>
        /// 获取本机的Ip地址
        /// </summary>
        /// <returns></returns>
        private static string GetHostIp()
        {
            if (string.IsNullOrWhiteSpace(_hostIp))
            {
                try
                {
                    //摘抄自阿波罗.NET SDK NetworkInterfaceManager.cs
                    var hostIp = NetworkInterface.GetAllNetworkInterfaces()
                        .Where(network => network.OperationalStatus == OperationalStatus.Up)
                        .Select(network => network.GetIPProperties())
                        .OrderByDescending(properties => properties.GatewayAddresses.Count)
                        .SelectMany(properties => properties.UnicastAddresses)
                        .FirstOrDefault(address => !IPAddress.IsLoopback(address.Address)
                                                   && address.Address.AddressFamily == AddressFamily.InterNetwork);

                    if (hostIp != null)
                        _hostIp = hostIp.Address.ToString();
                }
                catch
                {
                    _hostIp = "127.0.0.1";
                }
            }

            return _hostIp;
        }

        #endregion 私有方法
    }
}