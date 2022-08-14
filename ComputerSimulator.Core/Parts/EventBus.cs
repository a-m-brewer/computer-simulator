namespace ComputerSimulator.Core.Parts;

public interface IBus : IWireGroup<bool>
{
}

public class EventBus : WireGroup<bool>, IBus
{
    public EventBus(IWire2<bool>[] wires) : base(wires)
    {
    }
}