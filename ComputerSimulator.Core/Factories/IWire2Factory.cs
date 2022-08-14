using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IWire2Factory2
{
    IWire2<T> CreateWire<T>(T initialValue);
    IWireGroup<T> CreateGroup<T>(params IWire2<T>[] wires);
    IWireGroup<T> CreateGroup<T>(T initialValue);
    IWireGroup<T> CreateGroup<T>(T initialValue, int size);
    IBus CreateBus();
}