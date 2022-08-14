using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface INot : IComponent2
{
    IWire2<bool> Input { get; }
    IWire2<bool> Output { get; }
}

public class Not : ComponentBase2, INot
{
    public Not(
        IWire2<bool> input,
        IWire2<bool> output)
    {
        Input = input.SubscribeToValueChanged(HandleInputChanged);
        Output = output;
    }
    
    public IWire2<bool> Input { get; }

    public IWire2<bool> Output { get; }
    
    private void HandleInputChanged(object? sender, EventArgs eventArgs)
    {
        Output.Value = !Input.Value;
    }
}