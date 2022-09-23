using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public class WireFactory : IWireFactory
{
    private readonly ComputerSettings _computerSettings;
    private static readonly IWire<bool> _powerWire = new ReadOnlyWire<bool>(true, nameof(PowerWire));
    private static readonly IWire<bool> _offWire = new ReadOnlyWire<bool>(false, nameof(OffWire));

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

    public ISetEnableWireGroup<T> CreateSetEnableWireGroup<T>(T initialValue, int size, string? label = null)
    {
        return new SetEnableWireGroup<T>(
            size
                .InitArray<ISetEnableWire<T>>()
                .Fill(i => CreateSetEnableWire(initialValue, $"{label}-{i}")));
    }

    public ICaez<T> CreateCaez<T>(T initialValue, string? label = null)
    {
        return new Caez<T>(
            CreateWire(initialValue, CreateLabel(label, "c")),
            CreateWire(initialValue, CreateLabel(label, "a")),
            CreateWire(initialValue, CreateLabel(label, "e")),
            CreateWire(initialValue, CreateLabel(label, "z"))
        );
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

    public ISetEnableWire<T> CreateSetEnableWire<T>(T initialValue, string? label = null)
    {
        return new SetEnableWire<T>(
            CreateWire(initialValue, CreateLabel(label, "set")),
            CreateWire(initialValue, CreateLabel(label, "enable")));
    }

    public IWire<T>[] CreateWireSet<T>(T initialValue, int size, string? label = null)
    {
        return size
            .InitArray<IWire<T>>()
            .Fill(i => CreateWire(initialValue, $"{label}-{i}"));
    }
    
    private string? CreateLabel(string? label, string suffix)
    {
        return label == null ? null : $"{label}-{suffix}";
    }
}