using System.Collections;
using ComputerSimulator.Core.Enums;

namespace ComputerSimulator.Core.Parts;

public interface IWireGroup
{
}

public interface IWireGroup<T> : IWireGroup, IReadOnlyList<IWire2<T>>
{
    IWire2<T> this[OpCode index] { get; }
}

public class WireGroup<T> : IWireGroup<T>
{
    private readonly IList<IWire2<T>> _wires;

    public WireGroup(IList<IWire2<T>> wires)
    {
        _wires = wires;
    }

    public int Count => _wires.Count;
    
    public IEnumerator<IWire2<T>> GetEnumerator()
    {
        return _wires.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IWire2<T> this[int index] => _wires[index];

    public IWire2<T> this[OpCode index] => _wires[(int)index];
}