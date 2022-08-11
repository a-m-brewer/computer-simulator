using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Services;

namespace ComputerSimulator.Core.Circuits;

public abstract class CircuitBase : ComponentBase2
{
    private readonly IWireService _wireService;

    protected CircuitBase(IWireService wireService)
    {
        _wireService = wireService;
    }

    protected IWire2<T> CreateInternalWire<T>(string label, T initialValue)
    {
        return _wireService.Create(GenerateLabel(label), initialValue);
    }

    protected IWireGroup<T> CreateInternalWireGroup<T>(string label)
    {
        return _wireService.CreateGroup<T>(GenerateLabel(label));
    }
    
    protected IWireGroup<T> CreateInternalWireGroup<T>(string label, T initialValue)
    {
        return _wireService.CreateGroup(GenerateLabel(label), initialValue);
    }
    
    protected IWireGroup<T> CreateInternalWireGroup<T>(string label, T initialValue, int size)
    {
        return _wireService.CreateGroup(GenerateLabel(label), initialValue, size);
    }

    private string GenerateLabel(string label) => $"{GetType().Name}-{Id}-{label}";
}