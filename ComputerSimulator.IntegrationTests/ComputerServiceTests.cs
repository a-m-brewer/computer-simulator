using System;
using System.Threading;
using System.Threading.Tasks;
using ComputerSimulator.Core;
using FluentAssertions;
using NUnit.Framework;

namespace ComputerSimulator.IntegrationTests;

public class ComputerServiceTests
{
    [Test]
    public async Task StartRunsBlockingComputerLoopInBackground()
    {
        var computer = new BlockingComputer();
        var service = new ComputerService(computer);

        await service.StartAsync(CancellationToken.None);

        computer.Started.Wait(TimeSpan.FromSeconds(1)).Should().BeTrue();

        using var stopTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await service.StopAsync(stopTimeout.Token);

        computer.Disposed.Should().BeTrue();
    }

    private sealed class BlockingComputer : IComputer
    {
        public ManualResetEventSlim Started { get; } = new();

        public bool Disposed { get; private set; }

        public Task RunAsync(CancellationToken cancellationToken)
        {
            Started.Set();
            cancellationToken.WaitHandle.WaitOne();
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Disposed = true;
            Started.Dispose();
        }
    }
}
