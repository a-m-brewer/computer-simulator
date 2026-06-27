using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Peripherals.Display;

namespace ComputerSimulator.Core;

public interface IComputer : IDisposable
{
    Task RunAsync(CancellationToken cancellationToken);
}

public class Computer : IComputer
{
    // The CPU advances in quarter-clock sub-ticks; render every so many to keep the loop responsive
    // without paying for a full display scan too often.
    private const int UpdatesPerFrame = 400;

    private readonly IDisplayOutput _output;
    private readonly IComputerPart _computerPart;
    private readonly IDisplayAdapter _display;

    public Computer(IComponentFactory componentFactory, IDisplayOutput output)
    {
        _output = output;
        _computerPart = componentFactory.CreateComputerPart();
        _display = componentFactory.CreateDisplayAdapter(_computerPart.IoBus);
        _computerPart.IoBus.ConnectedComponents.Add(_display);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _output.Initialize(_display.Width, _display.Height);
        LoadProgram(DemoProgram.Build(_display.Width, _display.Height));

        var updates = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            _computerPart.Update();

            if (++updates % UpdatesPerFrame != 0)
            {
                continue;
            }

            _display.RenderFrame(_output);
            await Task.Delay(16, cancellationToken); // ~60 fps; also avoids busy-spinning once the program halts
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
