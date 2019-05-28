using System;
using Microsoft.Extensions.Configuration;

namespace Coder.Apollo.Sdk.Demo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddApollo(
                optionsConfig =>
                {
                    optionsConfig.AppId = "test.project";
                    optionsConfig.MetaServer = "http://eureka.aecg.com.cn"; //MetaServer的不同，可以决定是pro还是fat等
                    optionsConfig.Namespaces.Add("application");
                    optionsConfig.Namespaces.Add("developer");
                    optionsConfig.Namespaces.Add("infrastructure.configs");
                });
            var configuration = builder.Build();

            var serviceName = configuration.GetValue<string>("app.TEST");

            Console.WriteLine(serviceName);

            // appsettings.json 配置：
            //{
            //    "apollo": {
            //        "AppId": "test.project",
            //        "MetaServer": "http://eureka.aecg.com.cn",
            //        "Namespaces": ["application","developer","infrastructure.configs"]
            //    }
            //}

            // 程序中使用：
            //public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            //    WebHost.CreateDefaultBuilder(args)
            //        .ConfigureAppConfiguration(
            //            (hostingContext, builder) =>
            //            {
            //                builder.AddApollo(builder.Build().GetSection("apollo"));
            //            })
            //        .UseStartup<Startup>();

            Console.Read();
        }
    }
}