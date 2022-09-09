using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IAnd2 : IGate2
{
}

public class And2 : IAnd2
{
    public And2(
        IWire<bool> inputA, 
        IWire<bool> inputB,
        IWire<bool> output)
    {
        InputA = inputA;
        InputB = inputB;
        Output = output;
    }
    
    public IWire<bool> InputA { get; }

    public IWire<bool> InputB { get; }

    public IWire<bool> Output { get; }

    public void Update()
    {
        Output.Value = InputA.Value && InputB.Value;
    }
}