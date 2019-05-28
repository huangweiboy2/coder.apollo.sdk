1. #### 使用方式一：

   ```c#
   var builder = new ConfigurationBuilder();
   builder.AddApollo(optionsConfig =>{
                       optionsConfig.AppId = "test.project";
                       //MetaServer的不同，可以决定是pro还是fat等
                       optionsConfig.MetaServer = "http://eureka.aecg.com.cn";
                       optionsConfig.Namespaces.Add("application");
                       optionsConfig.Namespaces.Add("developer");
                       optionsConfig.Namespaces.Add("infrastructure.configs");
                   });
   var configuration = builder.Build();
   var serviceName = configuration.GetValue<string>("app.TEST");
   Console.WriteLine(serviceName);
   ```

2. ##### 使用方式二：

   ```c#
   appsettings.json 配置信息如下：
   {
       "apollo": {
           "AppId": "test.project",
           "MetaServer": "http://eureka.aecg.com.cn",
           "Namespaces": [
               "application",
               "developer",
               "infrastructure.configs"
           ]
       }
   }
   代码如下：
   public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
               WebHost.CreateDefaultBuilder(args)
       		.ConfigureAppConfiguration(
       		(hostingContext, builder) =>{
                builder.AddApollo(builder.Build().GetSection("apollo"));
               })
       		.UseStartup<Startup>();
   ```

   
