using MachineJob;
using MachineJob.Service;
using Serilog;

var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", true)
        .Build();

Log.Logger = new LoggerConfiguration()
      .ReadFrom.Configuration(configuration)
      .WriteTo.Console()
      .CreateLogger();

var loggerFactory = new LoggerFactory().AddSerilog(Log.Logger);

Quartz.Logging.LogProvider.SetCurrentLogProvider(new ConsoleLogProvider());
Modbus.Net.LogProvider.SetLogProvider(loggerFactory);

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, true));
    })
    .Build();

await host.RunAsync();

