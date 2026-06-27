using ComputerSimulator.Core;
using Microsoft.Extensions.Hosting;

namespace ComputerSimulator;

public class ComputerService : IHostedService
{
    private readonly IComputer _computer;
    private readonly CancellationTokenSource _cts = new();
    private Task? _runTask;

    public ComputerService(IComputer computer)
    {
        _computer = computer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // The simulation loop is blocking, so run it on a background thread.
        _runTask = _computer.RunAsync(_cts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cts.CancelAsync();

        if (_runTask is not null)
        {
            await Task.WhenAny(_runTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        _computer.Dispose();
    }
}
