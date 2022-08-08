using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IWire2Factory
{
    IWire2<T> Create<T>(string label, T initialValue);
    IWireGroup<T> CreateGroup<T>(string label, T initialValue);
    IWireGroup<T> CreateGroup<T>(string label, T initialValue, int size);
}