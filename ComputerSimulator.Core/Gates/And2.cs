using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public class And2
{
    private IWire2<bool> _input = DisconnectedWire<bool>.Instance;

    public Guid Id { get; } = Guid.NewGuid();
    
    public IWire2<bool> Input
    {
        get => _input;
        set => WireHelper.SetWire(ref _input, value, Id, HandleInputChanged);
    }

    public IWire2<bool> Output { get; set; } = DisconnectedWire<bool>.Instance;
    
    private void HandleInputChanged(bool newInputValue)
    {
        Output.Value = newInputValue;
    }
}