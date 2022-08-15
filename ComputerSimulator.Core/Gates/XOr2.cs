using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IXOr2 : IComponent2
{
    IWire2<bool> InputA { get; }
    IWire2<bool> InputB { get; }
    IWire2<bool> Output { get; }
}

public class XOr2 : IXOr2
{
    public XOr2(
        IWire2<bool> inputA,
        IWire2<bool> inputB,
        IWire2<bool> output)
    {
        InputA = inputA;
        InputB = inputB;
        Output = output;
    }
    
    public IWire2<bool> InputA { get; }
    public IWire2<bool> InputB { get; }
    public IWire2<bool> Output { get; }

    public void Update()
    {
        Output.Value = InputA.Value ^ InputB.Value;
    }
}