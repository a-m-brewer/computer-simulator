using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IWireFactory
{
    IWire<T> CreateWire<T>(T initialValue);

    IWireGroup<T> CreateGroup<T>(params IWire<T>[] wires);

    IWireGroup<T> CreateGroup<T>(T initialValue);

    IWireGroup<T> CreateGroup<T>(T initialValue, int size);

    IBus CreateBus();

    IOp CreateOp();

    IWire<T>[] CreateWireSet<T>(T initialValue);
    IWire<T>[] CreateWireSet<T>(T initialValue, int size);

    // A wire that is always true
    IWire<bool> PowerWire { get; }
    
    int WordSize { get; }
}