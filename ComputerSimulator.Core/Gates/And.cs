using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IAnd : IComponent2
{
    IWireGroup<bool> Inputs { get; set; }
    IWire2<bool> Output { get; set; }
}

public class And : IAnd
{
    private IWireGroup<bool> _inputs = DisconnectedWireGroup<bool>.Instance;

    public Guid Id { get; } = Guid.NewGuid();

    public IWireGroup<bool> Inputs
    {
        get => _inputs;
        set
        {
            WireGroupHelper.ReSubscribeWireValuesChanged(_inputs, value, HandleInputChanged);
            _inputs = value;
        }
    }

    public IWire2<bool> Output { get; set; } = DisconnectedWire<bool>.Instance;
    
    private void HandleInputChanged(object? sender, EventArgs eventArgs)
    {
        Output.Value = Inputs.All(a => a.Value);
    }
}