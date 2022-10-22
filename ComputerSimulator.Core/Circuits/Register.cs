using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IRegister : ICircuit
{
    IWire<bool> Set { get; }

    IWire<bool> Enable { get; }

    IWireGroup<bool> Inputs { get; }

    IWireGroup<bool> Outputs { get; }
    
    /// <summary>
    /// Purely for debug/testing purposes only. Do not use for any actual code
    /// </summary>
    IWireGroup<bool> StoredValue { get; }
}

public class Register : CircuitBase, IRegister
{
    private readonly IEnabler _enabler;
    private readonly IWord _word;

    public Register(
        IWire<bool> set,
        IWire<bool> enable,
        IWireGroup<bool> inputs,
        IWireGroup<bool> outputs,
        IComponentFactory componentFactory,
        IWireFactory wireFactory) : base(componentFactory, wireFactory)
    {
        StoredValue = WireFactory.CreateGroup(false);

        _word = ComponentFactory.CreateWord(inputs, StoredValue, set);
        _enabler = ComponentFactory.CreateEnabler(enable, StoredValue, outputs);
    }

    public IWire<bool> Set => _word.Set;

    public IWire<bool> Enable => _enabler.Enable;

    public IWireGroup<bool> Inputs => _word.Inputs;

    public IWireGroup<bool> Outputs => _enabler.Outputs;
    public IWireGroup<bool> StoredValue { get; }

    public void Update()
    {
        _word.Update();
        _enabler.Update();
    }
}