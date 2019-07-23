```c#
使用方式一：   

public class Program
{
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
	var serviceName = configuration.GetValue<string>("zxz");
	Console.WriteLine(serviceName);
}
```

```json
使用方式二：

配置文件：
{
  "apollo": {
    "AppId": "Aquarium.Logging",
     //MetaServer的不同，可以决定是pro还是fat等
    "MetaServer": "http://eureka.aecg.com.cn",
    "Namespaces": [
      "application",
      "developer",
      "infrastructure.configs"
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Trace"
    }
  },
  "AllowedHosts": "*"
}

Program.cs类中使用代码：
public static IWebHostBuilder CreateWebHostBuilder(string[] args)
{
     var host = WebHost.CreateDefaultBuilder(args)
						.ConfigureAppConfiguration(
                    (hostingContext, builder) =>
                    {
                        builder.AddApollo(builder.Build().GetSection("apollo"));
                    })
                .UseStartup<Startup>();
      return host;
}

Startup.cs类中启用Apollo配置组件日志功能代码（默认不启用）：
 public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
 {
      var loggerName = $"Apollo-{this.GetType().Namespace}-{Environment.MachineName}";
      var logger = loggerFactory.CreateLogger(loggerName);
      app.UseApolloConfigLogger(logger);
 }
```

```c#

	[Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ValuesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get(string key = "")
        {
            string title = _configuration.GetValue<string>(key);
            return new string[] {title};
        }
    }
```

