using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IWire2Factory2
{
    IWire2<T> CreateWire<T>(T initialValue);

    IWireGroup<T> CreateGroup<T>(params IWire2<T>[] wires);

    IWireGroup<T> CreateGroup<T>(T initialValue);

    IWireGroup<T> CreateGroup<T>(T initialValue, int size);

    IBus CreateBus();

    IOp CreateOp();

    IWire2<T>[] CreateWireSet<T>(T initialValue);
    IWire2<T>[] CreateWireSet<T>(T initialValue, int size);

    // A wire that is always true
    IWire2<bool> PowerWire { get; }
    
    int WordSize { get; }
}