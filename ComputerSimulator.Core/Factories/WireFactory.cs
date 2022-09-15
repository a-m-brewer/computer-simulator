using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public class WireFactory : IWireFactory
{
    private readonly ComputerSettings _computerSettings;
    private static readonly IWire<bool> _powerWire = new Wire<bool>(true, nameof(PowerWire));
    private static readonly IWire<bool> _offWire = new Wire<bool>(false, nameof(OffWire));

    public WireFactory(
        ComputerSettings computerSettings)
    {
        _computerSettings = computerSettings;
    }

    public IWire<T> CreateWire<T>(T initialValue, string? label = null)
    {
        return new Wire<T>(initialValue, label);
    }

    public IWireGroup<T> CreateGroup<T>(params IWire<T>[] wires)
    {
        return new WireGroup<T>(wires);
    }

    public IWireGroup<T> CreateGroup<T>(T initialValue, string? label = null)
    {
        return CreateGroup(initialValue, _computerSettings.WordSize, label);
    }

    public IBus CreateBus(string? label = null)
    {
        return new EventBus(CreateWireSet(false, _computerSettings.WordSize, label));
    }

    public IOp CreateOp(string? label = null)
    {
        return new Op(CreateWireSet(false, 3, label));
    }

    public IWire<T>[] CreateWireSet<T>(T initialValue, string? label = null)
    {
        return CreateWireSet(initialValue, _computerSettings.WordSize, label);
    }

    public IWire<bool> PowerWire => _powerWire;
    public IWire<bool> OffWire => _offWire;

    public int WordSize => _computerSettings.WordSize;

    public IWireGroup<T> CreateGroup<T>(T initialValue, int size, string? label = null)
    {
        return new WireGroup<T>(CreateWireSet(initialValue, size, label));
    }

    public IWire<T>[] CreateWireSet<T>(T initialValue, int size, string? label = null)
    {
        return size
            .InitArray<IWire<T>>()
            .Fill(i => CreateWire(initialValue, $"{label}-{i}"));
    }
}