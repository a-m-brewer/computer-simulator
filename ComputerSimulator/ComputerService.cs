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
        _runTask = Task.Run(() => _computer.RunAsync(_cts.Token), CancellationToken.None);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cts.CancelAsync();

        if (_runTask is not null)
        {
            var completed = await Task.WhenAny(_runTask, Task.Delay(Timeout.Infinite, cancellationToken));
            if (completed == _runTask)
            {
                try
                {
                    await _runTask;
                }
                catch (OperationCanceledException) when (_cts.IsCancellationRequested)
                {
                }
            }
        }

        _computer.Dispose();
    }
}
