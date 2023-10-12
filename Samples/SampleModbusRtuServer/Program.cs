using SampleModbusRtuServer;
using SampleModbusRtuServer.Service;
using Serilog;

IHost host = Host.CreateDefaultBuilder(args).UseWindowsService()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        var configuration = config
            .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", true)
            .AddEnvironmentVariables()
            .Build();

        Directory.SetCurrentDirectory(hostingContext.HostingEnvironment.ContentRootPath);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Log\\log..txt", Serilog.Events.LogEventLevel.Error, shared: true, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

        Quartz.Logging.LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
        Modbus.Net.LogProvider.SetLogProvider(loggerFactory);
    }
        )
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, true));
    })
    .Build();

await host.RunAsync();

