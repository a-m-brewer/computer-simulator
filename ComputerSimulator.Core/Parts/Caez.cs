namespace ComputerSimulator.Core.Parts;

public interface ICaez<T> : IWireGroup<T> where T : new()
{
    IWire<T> C { get; }
    IWire<T> A { get; }
    IWire<T> E { get; }
    IWire<T> Z { get; }
}

public class Caez<T> : WireGroup<T>, ICaez<T> where T : new()
{
    public Caez(
        IWire<T> c,
        IWire<T> a,
        IWire<T> e,
        IWire<T> z
        ) : base([c, a, e, z])
    {
    }

    public IWire<T> C => Wires[0];
    public IWire<T> A => Wires[1];
    public IWire<T> E => Wires[2];
    public IWire<T> Z => Wires[3];
}