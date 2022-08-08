using ComputerSimulator.Core.Models;

namespace ComputerSimulator.Core.Parts;

public interface IBus : IWireGroup<bool>
{
    int Length { get; }
}

public class MessageBrokerBus : WireGroup<bool>, IBus
{
    public MessageBrokerBus(ComputerSettings computerSettings)
    {
        for (var i = 0; i < computerSettings.WordSize; i++)
        {
            SetWire(i, DisconnectedWire<bool>.Instance);
        }
    }

    public int Length => Wires.Count;
}

public class DisconnectedBus : DisconnectedWireGroup<bool>, IBus
{
    public static IBus Instance => new DisconnectedBus();

    public int Length => 0;
}