using ComputerSimulator.Core;
using Microsoft.Extensions.Hosting;

namespace ComputerSimulator;

public class ComputerService : IHostedService
{
    private readonly IComputer _computer;

    public ComputerService(IComputer computer)
    {
        _computer = computer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _computer.Run();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _computer.Dispose();

        return Task.CompletedTask;
    }
}