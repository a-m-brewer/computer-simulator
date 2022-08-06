using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface INAnd : IComponent2
{
    IWireGroup<bool> Inputs { get; set; }
    IWire2<bool> Output { get; set; }
}

public class NAnd : ComponentBase2, INAnd
{
    private readonly IAnd _andGate;
    private readonly INot _notGate;

    public NAnd(IAnd andGate, INot notGate)
    {
        _andGate = andGate;
        _notGate = notGate;
        
        // TODO: make a way of having internal wires
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