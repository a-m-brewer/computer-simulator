using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IOr : IComponent2
{
    IWireGroup<bool> Inputs { get; }
    
    IWire2<bool> Output { get; }
}

public class Or : IOr
{
    public Or(IWireGroup<bool> inputs, IWire2<bool> output)
    {
        Inputs = inputs;
        Output = output;
    }

    public IWireGroup<bool> Inputs { get; }
    public IWire2<bool> Output { get; }
    
    public void Update()
    {
        Output.Value = Inputs.Any(a => a.Value);
    }
}