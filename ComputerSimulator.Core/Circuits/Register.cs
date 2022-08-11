using ComputerSimulator.Core.Factories;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Services;

namespace ComputerSimulator.Core.Circuits;

public interface IRegister : IComponent2
{
    IWire2<bool> Set { get; set; }

    IWire2<bool> Enable { get; set; }

    IWireGroup<bool> Inputs { get; set; }

    IWireGroup<bool> Outputs { get; set; }
}

public class Register : CircuitBase, IRegister
{
    private readonly IEnabler _enabler;
    private readonly IWord _word;

    public Register(
        IEnabler enabler,
        IWord word,
        IWireService wireService) : base(wireService)
    {
        _enabler = enabler;
        _word = word;

        var internalGroup = CreateInternalWireGroup("word-to-enabler", false);
        _word.Outputs = internalGroup;
        _enabler.Inputs = internalGroup;
    }

    public IWire2<bool> Set
    {
        get => _word.Set;
        set => _word.Set = value;
    }

    public IWire2<bool> Enable
    {
        get => _enabler.Enable;
        set => _enabler.Enable = value;
    }

    public IWireGroup<bool> Inputs
    {
        get => _word.Inputs;
        set => _word.Inputs = value;
    }

    public IWireGroup<bool> Outputs
    {
        get => _enabler.Outputs;
        set => _enabler.Outputs = value;
    }
}