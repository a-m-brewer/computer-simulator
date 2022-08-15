using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public class WireFactory : IWire2Factory2
{
    private readonly ComputerSettings _computerSettings;

    public WireFactory(
        ComputerSettings computerSettings)
    {
        _computerSettings = computerSettings;
    }
    
    public IWire2<T> CreateWire<T>(T initialValue)
    {
        return new Wire<T>(initialValue);
    }

    public IWireGroup<T> CreateGroup<T>(params IWire2<T>[] wires)
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

    public IWireGroup<T> CreateGroup<T>(T initialValue, int size)
    {
        return new WireGroup<T>(CreateWireSet(initialValue, size));
    }

    private IWire2<T>[] CreateWireSet<T>(T initialValue, int size)
    {
        return size
            .InitArray<IWire2<T>>()
            .Fill(() => CreateWire(initialValue));
    }
}