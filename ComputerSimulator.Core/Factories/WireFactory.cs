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

    public IWire<T> CreateWire<T>(string? label = null) where T : new()
    {
        return new Wire<T>(label);
    }

    public IWireGroup<T> CreateGroup<T>(params IWire<T>[] wires) where T : new()
    {
        return new WireGroup<T>(wires);
    }

    public IWireGroup<T> CreateGroup<T>(string? label = null) where T : new()
    {
        return CreateGroup<T>(_computerSettings.WordSize, label);
    }

    public ISetEnableWireGroup<T> CreateSetEnableWireGroup<T>(int size, string? label = null) where T : new()
    {
        return new SetEnableWireGroup<T>(
            size
                .InitArray<ISetEnableWire<T>>()
                .Fill(i => CreateSetEnableWire<T>($"{label}-{i}")));
    }

    public ICaez<T> CreateCaez<T>(string? label = null) where T : new()
    {
        return new Caez<T>(
            CreateWire<T>(CreateLabel(label, "c")),
            CreateWire<T>(CreateLabel(label, "a")),
            CreateWire<T>(CreateLabel(label, "e")),
            CreateWire<T>(CreateLabel(label, "z"))
        );
    }

    public IIoBus CreateIoBus(string? label = null)
    {
        return new IoBus(
            CreateBus(CreateLabel(label, "cpu-bus")),
            CreateWire<bool>(CreateLabel(label, "i/o")),
            CreateWire<bool>(CreateLabel(label, "Data/Address")),
            CreateSetEnableWire<bool>(CreateLabel(label, "i/o-clk"))
        );
    }

    public IBus CreateBus(string? label = null)
    {
        return new EventBus(CreateWireSet<bool>(_computerSettings.WordSize, label));
    }

    public IOp CreateOp(string? label = null)
    {
        return new Op(CreateWireSet<bool>(3, label));
    }

    public IWire<T>[] CreateWireSet<T>(string? label = null) where T : new()
    {
        return CreateWireSet<T>(_computerSettings.WordSize, label);
    }

    public IWire<bool> PowerWire => _powerWire;
    public IWire<bool> OffWire => _offWire;

    public int WordSize => _computerSettings.WordSize;

    public IWireGroup<T> CreateGroup<T>(int size, string? label = null) where T : new()
    {
        return new WireGroup<T>(CreateWireSet<T>(size, label));
    }

    public ISetEnableWire<T> CreateSetEnableWire<T>(string? label = null) where T : new()
    {
        return new SetEnableWire<T>(
            CreateWire<T>(CreateLabel(label, "set")),
            CreateWire<T>(CreateLabel(label, "enable")));
    }

    public IWire<T>[] CreateWireSet<T>(int size, string? label = null) where T : new()
    {
        return size
            .InitArray<IWire<T>>()
            .Fill(i => CreateWire<T>($"{label}-{i}"));
    }
    
    private string? CreateLabel(string? label, string suffix)
    {
        return label == null ? null : $"{label}-{suffix}";
    }
}