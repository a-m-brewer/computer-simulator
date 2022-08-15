using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IEnabler : IComponent2
{
    IWire2<bool> Enable { get; }
    IWireGroup<bool> Inputs { get; }
    IWireGroup<bool> Outputs { get; }
}

public class Enabler : CircuitBase, IEnabler
{
    // Wires
    private readonly IWireGroup<bool> _internalOutput;
    
    // Gates
    // ReSharper disable once NotAccessedField.Local
    private readonly IAnd[] _ands;

    public Enabler(
        IWire2<bool> enable,
        IWireGroup<bool> inputs,
        IWireGroup<bool> outputs,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        Enable = enable;
        Inputs = inputs;
        Outputs = outputs;
        
        _internalOutput = WireFactory.CreateGroup(false);
        _internalOutput.WireValuesChanged += InternalOutputOnWireValuesChanged;

        _ands = inputs.Count
            .InitArray<IAnd>()
            .Fill(i => new And(WireFactory.CreateGroup(Inputs[i], Enable), _internalOutput[i]));
    }

    public IWire2<bool> Enable { get; }

    public IWireGroup<bool> Inputs { get; }

    public IWireGroup<bool> Outputs { get; }

    private void InternalOutputOnWireValuesChanged(object? sender, int e)
    {
        if (Enable.Value)
        {
            Outputs[e].Value = _internalOutput[e].Value;
        }
    }
}