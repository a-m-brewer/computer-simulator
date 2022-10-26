using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IWireFactory
{
    IWire<T> CreateWire<T>(string? label = null) where T : new();

    IWireGroup<T> CreateGroup<T>(params IWire<T>[] wires) where T : new();

    IWireGroup<T> CreateGroup<T>(string? label = null) where T : new();

    IWireGroup<T> CreateGroup<T>(int size, string? label = null) where T : new();

    ISetEnableWire<T> CreateSetEnableWire<T>(string? label = null) where T : new();
    ISetEnableWireGroup<T> CreateSetEnableWireGroup<T>(int size, string? label = null) where T : new();

    ICaez<T> CreateCaez<T>(string? label = null) where T : new();
    
    IIoBus CreateIoBus(string? label = null);

    IBus CreateBus(string? label = null);

    IOp CreateOp(string? label = null);

    IWire<T>[] CreateWireSet<T>(string? label = null) where T : new();
    IWire<T>[] CreateWireSet<T>(int size, string? label = null) where T : new();

    // A wire that is always true
    IWire<bool> PowerWire { get; }
    
    // A wire that is always false
    IWire<bool> OffWire { get; }
    
    int WordSize { get; }
}