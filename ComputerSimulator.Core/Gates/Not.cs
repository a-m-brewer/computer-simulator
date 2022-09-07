using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface INot : IComponent2
{
    IWire<bool> Input { get; }
    IWire<bool> Output { get; }
}

public class Not : INot
{
    public Not(
        IWire<bool> input,
        IWire<bool> output)
    {
        Input = input;
        Output = output;
    }
    
    public IWire<bool> Input { get; }

    public IWire<bool> Output { get; }

    public void Update()
    {
        Output.Value = !Input.Value;
    }
}