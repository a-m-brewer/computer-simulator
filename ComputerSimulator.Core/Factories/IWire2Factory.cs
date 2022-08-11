using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public interface IWire2Factory2
{
    IWire2<T> Create<T>(string label, T initialValue);
    IWireGroup<T> CreateGroup<T>(string label);
    IWireGroup<T> CreateGroup<T>(string label, T initialValue);
    IWireGroup<T> CreateGroup<T>(string label, T initialValue, int size);
}