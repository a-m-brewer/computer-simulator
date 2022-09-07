using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public class WireFactory : IWireFactory
{
    private readonly ComputerSettings _computerSettings;
    private static readonly IWire<bool> _powerWire = new Wire<bool>(true);

    public WireFactory(
        ComputerSettings computerSettings)
    {
        _computerSettings = computerSettings;
    }
    
    public IWire<T> CreateWire<T>(T initialValue)
    {
        return new Wire<T>(initialValue);
    }

    public IWireGroup<T> CreateGroup<T>(params IWire<T>[] wires)
    {
        return new WireGroup<T>(wires);
    }

    public IWireGroup<T> CreateGroup<T>(T initialValue)
    {
        return CreateGroup(initialValue, _computerSettings.WordSize);
    }

    public IBus CreateBus()
    {
        return new EventBus(CreateWireSet(false, _computerSettings.WordSize));
    }

    public IOp CreateOp()
    {
        return new Op(CreateWireSet(false, 3));
    }

    public IWire<T>[] CreateWireSet<T>(T initialValue)
    {
        return CreateWireSet(initialValue, _computerSettings.WordSize);
    }

    public IWire<bool> PowerWire => _powerWire;

    public int WordSize => _computerSettings.WordSize;

    public IWireGroup<T> CreateGroup<T>(T initialValue, int size)
    {
        return new WireGroup<T>(CreateWireSet(initialValue, size));
    }

    public IWire<T>[] CreateWireSet<T>(T initialValue, int size)
    {
        return size
            .InitArray<IWire<T>>()
            .Fill(() => CreateWire(initialValue));
    }
}