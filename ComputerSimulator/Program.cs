using ComputerSimulator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

builder.ConfigureServices((host, services) => new Startup(host.Configuration).ConfigureServices(services));

using var host = builder.Build();

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("Bye...");
    cts.Cancel();
    e.Cancel = true;
};

await host.RunAsync(cts.Token);