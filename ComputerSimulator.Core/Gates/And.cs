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
        set => WireGroupHelper.SetWireGroup(ref _inputs, value, Id, HandleInputChanged);
    }

    public IWire2<bool> Output { get; set; } = DisconnectedWire<bool>.Instance;
    
    private void HandleInputChanged(IEnumerable<bool> wireValues)
    {
        Output.Value = wireValues.All(a => a);
    }
}