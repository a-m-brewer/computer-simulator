using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IOr : IComponent
{
    IWireGroup<bool> Inputs { get; }
    
    IWire<bool> Output { get; }
}

public class Or : IOr
{
    public Or(IWireGroup<bool> inputs, IWire<bool> output)
    {
        Inputs = inputs;
        Output = output;
    }

    public IWireGroup<bool> Inputs { get; }
    public IWire<bool> Output { get; }
    
    public void Update()
    {
        for (var i = 0; i < Inputs.Count; i++)
        {
            if (!Inputs[i].Value)
            {
                continue;
            }

            Output.Value = true;
            return;
        }

        Output.Value = false;
    }
}
