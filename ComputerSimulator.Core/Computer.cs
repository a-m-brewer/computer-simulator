using Avoid.MessageBroker;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core;

public interface IComputer : IDisposable
{
    public void Run();
}

public class Computer  : IComputer
{
    private readonly IRam _ram;
    private readonly IBus _ioBus;
    private readonly IBus _marBus;
    private readonly IWire2Factory _wire2Factory;

    public Computer(
        IRam ram,
        IBus ioBus,
        IBus marBus,
        IWire2Factory wire2Factory)
    {
        _ram = ram;
        _ioBus = ioBus;
        _marBus = marBus;
        _wire2Factory = wire2Factory;
    }
    
    public void Run()
    {
        _marBus.SetWires(_wire2Factory.CreateGroup("mar_bus", false));
        
        _ram.MarInputBus = _marBus;
        _ram.MarSet = _wire2Factory.Create("mar_set", false);

        _ram.Io = _ioBus;
        _ram.Set = _wire2Factory.Create("ram_set", false);
        _ram.Enable = _wire2Factory.Create("ram_enable", false);
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}