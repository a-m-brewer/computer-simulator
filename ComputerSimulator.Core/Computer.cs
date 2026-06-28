using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Peripherals.Display;
using Microsoft.Extensions.Logging;

namespace ComputerSimulator.Core;

public interface IComputer : IDisposable
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class Computer : IComputer
{
    private readonly IDisplayOutput _output;
    private readonly IComputerPart _computerPart;
    private readonly IDisplayAdapter _display;
    private readonly ComputerSettings _settings;
    private readonly ILogger<Computer> _logger;

    public Computer(IComponentFactory componentFactory, IDisplayOutput output, ComputerSettings settings, ILogger<Computer> logger)
    {
        _output = output;
        _settings = settings;
        _logger = logger;
        _computerPart = componentFactory.CreateComputerPart();
        _display = componentFactory.CreateDisplayAdapter(_computerPart.IoBus);
        _computerPart.IoBus.ConnectedComponents.Add(_display);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting computer simulation with settings: {Settings}", _settings);

        _output.Initialize(_display.Width, _display.Height);
        LoadProgram(DemoProgram.Build(_display.Width, _display.Height));

        var updates = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            _computerPart.Update();

            if (++updates % _settings.CpuUpdatesPerFrame != 0)
            {
                continue;
            }

            _display.RenderFrame(_output);
            if (_settings.DisplayFrameDelayMs > 0)
            {
                await Task.Delay(_settings.DisplayFrameDelayMs, cancellationToken);
            }
        }
    }

    private void LoadProgram(IReadOnlyList<bool[]> program)
    {
        for (var address = 0; address < program.Count; address++)
        {
            _computerPart.Ram
                .GetSlot(address & 0xFF, address >> 8)
                .Memory
                .SetRegisterValue(program[address]);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
