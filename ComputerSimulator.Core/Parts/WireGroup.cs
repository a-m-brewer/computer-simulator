using System.Collections;
using ComputerSimulator.Core.Enums;

namespace ComputerSimulator.Core.Parts;

public interface IWireGroup
{
}

public interface IWireGroup<T> : IWireGroup, IReadOnlyList<IWire<T>>
{
    IWire<T> this[OpCode index] { get; }
}

public class WireGroup<T> : IWireGroup<T>
{
    protected readonly IList<IWire<T>> Wires;

    public WireGroup(IList<IWire<T>> wires)
    {
        Wires = wires;
    }

    public int Count => Wires.Count;
    
    public IEnumerator<IWire<T>> GetEnumerator()
    {
        return Wires.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IWire<T> this[int index] => Wires[index];

    public IWire<T> this[OpCode index] => Wires[(int)index];
}