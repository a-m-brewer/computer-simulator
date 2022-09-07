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
    private readonly IList<IWire<T>> _wires;

    public WireGroup(IList<IWire<T>> wires)
    {
        _wires = wires;
    }

    public int Count => _wires.Count;
    
    public IEnumerator<IWire<T>> GetEnumerator()
    {
        return _wires.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IWire<T> this[int index] => _wires[index];

    public IWire<T> this[OpCode index] => _wires[(int)index];
}