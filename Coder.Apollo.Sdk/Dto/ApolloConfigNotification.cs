using Newtonsoft.Json;

namespace Coder.Apollo.Sdk.Dto
{
    public class ApolloConfigNotification
    {
        /// <summary>
        ///  Namespace的名字
        /// </summary>
        [JsonProperty("namespaceName")]
        public string NamespaceName { get; set; }

        /// <summary>
        /// 通知Id 默认-1
        /// </summary>
        [JsonProperty("notificationId")]
        public long NotificationId { get; set; } = -1;
    }
}