using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public class And2
{
    private IWire2 _input = DisconnectedWire.Instance;

    public IWire2 Input
    {
        get => _input;
        set
        {
            _input = value;
            _input.ConnectOutput(HandleInputChanged);
        }
    }

    public IWire2 Output { get; set; } = DisconnectedWire.Instance;
    
    private void HandleInputChanged(bool newInputValue)
    {
        Output.Value = newInputValue;
    }
}