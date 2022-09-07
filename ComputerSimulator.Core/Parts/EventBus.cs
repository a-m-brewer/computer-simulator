namespace ComputerSimulator.Core.Parts;

public interface IBus : IWireGroup<bool>
{
}

public class EventBus : WireGroup<bool>, IBus
{
    public EventBus(IWire<bool>[] wires) : base(wires)
    {
    }
}