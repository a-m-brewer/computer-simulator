using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Gates;

public interface IAnd : IComponent2
{
    IWire2<bool> Output { get; set; }
}

public class And : IAnd
{
    public IWire2<bool> Output { get; set; } = new DisconnectedWire<bool>();
}