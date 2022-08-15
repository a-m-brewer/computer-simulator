using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IRegister : ICircuit
{
    IWire2<bool> Set { get; }

    IWire2<bool> Enable { get; }

    IWireGroup<bool> Inputs { get; }

    IWireGroup<bool> Outputs { get; }
    
    /// <summary>
    /// Purely for debug/testing purposes only. Do not use for any actual code
    /// </summary>
    bool[] StoredValues { get; }
}

public class Register : CircuitBase, IRegister
{
    private readonly IEnabler _enabler;
    private readonly IWord _word;
    private readonly IWireGroup<bool> _internalGroup;

    public Register(
        IWire2<bool> set,
        IWire2<bool> enable,
        IWireGroup<bool> inputs,
        IWireGroup<bool> outputs,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        _internalGroup = WireFactory.CreateGroup(false);

        _word = ComponentFactory.CreateWord(inputs, _internalGroup, set);
        _enabler = ComponentFactory.CreateEnabler(enable, _internalGroup, outputs);
    }

    public IWire2<bool> Set => _word.Set;

    public IWire2<bool> Enable => _enabler.Enable;

    public IWireGroup<bool> Inputs => _word.Inputs;

    public IWireGroup<bool> Outputs => _enabler.Outputs;

    public bool[] StoredValues => _internalGroup
        .Select(s => s.Value)
        .ToArray();

    public void Update()
    {
        _word.Update();
        _enabler.Update();
    }
}