using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IAnd : IComponent2
{
    IWireGroup<bool> Inputs { get; }
    IWire2<bool> Output { get; }
}

public class And : IAnd
{
    public And(
        IWireGroup<bool> inputs,
        IWire2<bool> output)
    {
        Inputs = inputs.SubscribeToWireValuesChanged(HandleInputChanged);
        Output = output;
    }
    
    public IWireGroup<bool> Inputs { get; }

    public IWire2<bool> Output { get; }
    
    private void HandleInputChanged(object? sender, int index)
    {
        for (var i = 0; i < Inputs.Count; i++)
        {
            if (Inputs.GetValue(i))
            {
                continue;
            }

            Output.Value = false;
            return;
        }

        Output.Value = true;
    }
}