using ComputerSimulator;
using Microsoft.Extensions.Hosting;

using var host = args.BuildHost();

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

await host.RunAsync(cts.Token);
