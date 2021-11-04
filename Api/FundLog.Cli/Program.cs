using FundLog.ObjectGraph;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Typin;
using Typin.Modes;

namespace FundLog.Cli;

public class Program
{
  public static async Task Main(string[] args) => await new CliApplicationBuilder()
    .AddCommandsFromThisAssembly()
    .UseInteractiveMode()
    .UseLamar(services =>
    {
      var builder = new ConfigurationBuilder();
      builder.AddJsonFile("appsettings.json");
      builder.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true);
      IConfigurationRoot config = builder.Build();

      var root = new CompositionRoot(new DomainConfig
      {
        UseSignalRClient = true,
        // Needed only if UseSignalRClient = false
        ConnectionString = config.GetSection("ConnectionStrings")["DefaultConnection"], 
      });
      services.AddLamar(root.Registry);
    })
    .UseExceptionHandler<ExceptionHandler>()
    .Build()
    .RunAsync();
}