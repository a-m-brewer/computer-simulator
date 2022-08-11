using ComputerSimulator.Core.Models;

namespace ComputerSimulator.Core.Parts;

public interface IBus : IWireGroup<bool>
{
    int Length { get; }

    void SetWires(IWireGroup<bool> wires);
}

public class MessageBrokerBus : WireGroup<bool>, IBus
{
    private readonly ComputerSettings _computerSettings;

    public MessageBrokerBus(ComputerSettings computerSettings) : base()
    {
        _computerSettings = computerSettings;
        for (var i = 0; i < computerSettings.WordSize; i++)
        {
            SetWire(i, DisconnectedWire<bool>.Instance);
        }
    }

    public int Length => Wires.Count;
    public void SetWires(IWireGroup<bool> wires)
    {
        if (wires.Count != _computerSettings.WordSize)
        {
            throw new ArgumentException($"Wires: {wires.Count} must be equal to word size: {_computerSettings.WordSize}", nameof(wires));
        }
        
        for (var i = 0; i < wires.Count; i++)
        {
            SetWire(i, wires[i]);
        }
    }
}

public class DisconnectedBus : DisconnectedWireGroup<bool>, IBus
{
    public static IBus Instance => new DisconnectedBus();

    public int Length => 0;
    public void SetWires(IWireGroup<bool> wires)
    {
    }
}