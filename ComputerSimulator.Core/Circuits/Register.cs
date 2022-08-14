using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Circuits;

public interface IRegister : IComponent2
{
    IWire2<bool> Set { get; }

    IWire2<bool> Enable { get; }

    IWireGroup<bool> Inputs { get; }

    IWireGroup<bool> Outputs { get; }
}

public class Register : CircuitBase, IRegister
{
    private readonly IEnabler _enabler;
    private readonly IWord _word;

    public Register(
        IWire2<bool> set,
        IWire2<bool> enable,
        IWireGroup<bool> inputs,
        IWireGroup<bool> outputs,
        IComponentFactory2 componentFactory,
        IWire2Factory2 wireFactory) : base(componentFactory, wireFactory)
    {
        var internalGroup = WireFactory.CreateGroup(false);

        _word = ComponentFactory.CreateWord(inputs, internalGroup, set);
        _enabler = ComponentFactory.CreateEnabler(enable, internalGroup, outputs);
    }

    public IWire2<bool> Set => _word.Set;

    public IWire2<bool> Enable => _enabler.Enable;

    public IWireGroup<bool> Inputs => _word.Inputs;

    public IWireGroup<bool> Outputs => _enabler.Outputs;
}