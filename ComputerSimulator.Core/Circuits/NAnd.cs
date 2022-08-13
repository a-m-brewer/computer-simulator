using ComputerSimulator.Core.Gates;
using ComputerSimulator.Core.Parts;
using ComputerSimulator.Core.Services;

namespace ComputerSimulator.Core.Circuits;

public interface INAnd : IComponent2
{
    IWireGroup<bool> Inputs { get; set; }
    IWire2<bool> Output { get; set; }
}

public class NAnd : CircuitBase, INAnd
{
    private readonly IAnd _andGate;
    private readonly INot _notGate;

    public NAnd(
        IAnd andGate,
        INot notGate,
        IWireService wireService)
    : base(wireService)
    {
        _andGate = andGate;
        _notGate = notGate;

        var andToNot = CreateInternalWire("and-output-to-not-input", false);
        _andGate.Output = andToNot;
        _notGate.Input = andToNot;
    }

    public IWireGroup<bool> Inputs
    {
        get => _andGate.Inputs;
        set => _andGate.Inputs = value;
    }

    public IWire2<bool> Output
    {
        get => _notGate.Output;
        set => _notGate.Output = value;
    }
}