using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface INot : IComponent2
{
    IWire2<bool> Input { get; }
    IWire2<bool> Output { get; }
}

public class Not : INot
{
    public Not(
        IWire2<bool> input,
        IWire2<bool> output)
    {
        Input = input;
        Output = output;
    }
    
    public IWire2<bool> Input { get; }

    public IWire2<bool> Output { get; }

    public void Update()
    {
        Output.Value = !Input.Value;
    }
}