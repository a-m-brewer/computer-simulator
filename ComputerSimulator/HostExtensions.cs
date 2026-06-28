using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComputerSimulator;

public static class HostExtensions
{
    public static IHost BuildHost(this string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.SetBasePath(AppContext.BaseDirectory);
                configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                configuration.AddJsonFile(
                    $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true,
                    reloadOnChange: true);
                configuration.AddEnvironmentVariables();
                configuration.AddCommandLine(args);
            })
            .UseDefaultServiceProvider(options => options.ValidateScopes = true);

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureServices((host, services) => new Startup(host.Configuration).ConfigureServices(services));

        return builder.Build();
    }
}
