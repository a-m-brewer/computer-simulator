using ComputerSimulator.Core.Circuits;
using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;
using Microsoft.Extensions.Logging;

namespace ComputerSimulator.Core;

public interface IComputer : IDisposable
{
    public void Run();
}

public class Computer  : IComputer
{
    private readonly ComputerSettings _computerSettings;
    private readonly IRam _ram;
    private readonly ILogger<Computer> _logger;

    public Computer(
        ComputerSettings computerSettings,
        IRam ram,
        ILogger<Computer> logger)
    {
        _computerSettings = computerSettings;
        _ram = ram;
        _logger = logger;
    }
    
    public void Run()
    {
        var address = 65535.ToBinaryBools(_computerSettings.WordSize);
        
        _ram.InputBus.
    }
    
    public void Dispose()
    {
        _ram.Dispose();
        GC.SuppressFinalize(this);
    }
}