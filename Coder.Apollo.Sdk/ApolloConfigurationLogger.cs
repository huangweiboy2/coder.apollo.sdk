using Microsoft.Extensions.Logging;

namespace Coder.Apollo.Sdk
{
    internal class ApolloConfigurationLogger
    {
        internal static ILogger Logger;

        /// <summary>
        /// 初始化日志
        /// </summary>
        /// <param name="logger"></param>
        internal static void Init(ILogger logger)
        {
            Logger = logger;
        }
    }
}