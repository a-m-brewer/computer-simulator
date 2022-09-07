using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IOr2 : IComponent2
{
    IWire<bool> InputA { get; }
    IWire<bool> InputB { get; }
    IWire<bool> Output { get; }
}

public class Or2 : IOr2
{
    public Or2(
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
        Output.Value = InputA.Value || InputB.Value;
    }
}