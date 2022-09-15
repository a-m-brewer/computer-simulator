using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IWireFactory
{
    IWire<T> CreateWire<T>(T initialValue, string? label = null);

    IWireGroup<T> CreateGroup<T>(params IWire<T>[] wires);

    IWireGroup<T> CreateGroup<T>(T initialValue, string? label = null);

    IWireGroup<T> CreateGroup<T>(T initialValue, int size, string? label = null);

    IBus CreateBus(string? label = null);

    IOp CreateOp(string? label = null);

    IWire<T>[] CreateWireSet<T>(T initialValue, string? label = null);
    IWire<T>[] CreateWireSet<T>(T initialValue, int size, string? label = null);

    // A wire that is always true
    IWire<bool> PowerWire { get; }
    
    // A wire that is always false
    IWire<bool> OffWire { get; }
    
    int WordSize { get; }
}