using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface INot : IComponent2
{
    IWire2<bool> Input { get; set; }
    IWire2<bool> Output { get; set; }
}

public class Not : ComponentBase2, INot
{
    private IWire2<bool> _input = DisconnectedWire<bool>.Instance;

    public IWire2<bool> Input
    {
        get => _input;
        set => WireHelper.SetWire(ref _input, value, Id, HandleInputChanged);
    }

    public IWire2<bool> Output { get; set; }  = DisconnectedWire<bool>.Instance;
    
    private void HandleInputChanged(bool newValue)
    {
        Output.Value = !newValue;
    }
}