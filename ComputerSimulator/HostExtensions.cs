using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ComputerSimulator;

public static class HostExtensions
{
    public static IHost BuildHost(this string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });

        builder.ConfigureServices((host, services) => new Startup(host.Configuration).ConfigureServices(services));

        return builder.Build();
    }
}