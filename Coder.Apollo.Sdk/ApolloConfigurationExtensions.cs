using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Coder.Apollo.Sdk
{
    public static class AspNetExtensions
    {
        #region Apollo配置构建相关

        /// <summary>
        /// 构建Apollo配置
        /// </summary>
        public static IConfigurationBuilder AddApollo(this IConfigurationBuilder builder, IConfigurationSection apolloConfiguration)
            => builder.AddApollo(apolloConfiguration.Get<ApolloOptions>());

        /// <summary>
        /// 构建Apollo配置
        /// </summary>
        public static IConfigurationBuilder AddApollo(this IConfigurationBuilder builder, Action<ApolloOptions> optionsConfig)
        {
            var options = new ApolloOptions();
            optionsConfig.Invoke(options);
            return builder.Add(new ApolloConfigurationSource(options));
        }

        private static IConfigurationBuilder AddApollo(this IConfigurationBuilder builder, ApolloOptions apolloOptions)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (apolloOptions == null)
            {
                throw new ArgumentException(nameof(apolloOptions));
            }

            return builder.Add(new ApolloConfigurationSource(apolloOptions));
        }

        #endregion Apollo配置构建相关

        #region 启用Apollo配置日志功能

        public static void UseApolloConfigLogger(this IApplicationBuilder app, ILogger logger)
        {
            ApolloConfigurationLogger.Init(logger);
        }

        #endregion 启用Apollo配置日志功能
    }
}