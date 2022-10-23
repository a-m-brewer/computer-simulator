using ComputerSimulator.Core.Extensions;
using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IEnabler : ICircuit
{
    IWire<bool> Enable { get; }
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
        IWire<bool> enable,
        IWireGroup<bool> inputs,
        IWireGroup<bool> outputs,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        Enable = enable;
        Inputs = inputs;
        Outputs = outputs;
        
        _internalOutput = WireFactory.CreateGroup(false, outputs.Count);

        _ands = inputs.Count
            .InitArray<IAnd>()
            .Fill(i => new And(WireFactory.CreateGroup(Inputs[i], Enable), _internalOutput[i]));
    }

    public IWire<bool> Enable { get; }

    public IWireGroup<bool> Inputs { get; }

    public IWireGroup<bool> Outputs { get; }

    public void Update()
    {
        for (var i = 0; i < _ands.Length; i++)
        {
            _ands[i].Update();
        }

        if (!Enable.Value)
        {
            return;
        }
        
        for (var i = 0; i < _internalOutput.Count; i++)
        {
            Outputs[i].Value = _internalOutput[i].Value;
        }
    }
}