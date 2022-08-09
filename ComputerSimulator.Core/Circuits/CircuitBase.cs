using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public abstract class CircuitBase : ComponentBase2
{
    private readonly IWire2Factory _wireFactory;

    protected CircuitBase(IWire2Factory wireFactory)
    {
        _wireFactory = wireFactory;
    }

    protected IWire2<T> CreateInternalWire<T>(string label, T initialValue)
    {
        return _wireFactory.Create(GenerateLabel(label), initialValue);
    }

    protected IWireGroup<T> CreateInternalWireGroup<T>()
    {
        return _wireFactory.CreateGroup<T>();
    }
    
    protected IWireGroup<T> CreateInternalWireGroup<T>(string label, T initialValue)
    {
        return _wireFactory.CreateGroup(GenerateLabel(label), initialValue);
    }
    
    protected IWireGroup<T> CreateInternalWireGroup<T>(string label, T initialValue, int size)
    {
        return _wireFactory.CreateGroup(GenerateLabel(label), initialValue, size);
    }

    private string GenerateLabel(string label) => $"{GetType().Name}-{Id}-{label}";
}