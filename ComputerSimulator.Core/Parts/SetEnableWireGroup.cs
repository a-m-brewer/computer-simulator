using System.Collections;

namespace ComputerSimulator.Core.Parts;

public interface ISetEnableWireGroup
{
}

public interface ISetEnableWireGroup<T> : ISetEnableWireGroup, IReadOnlyList<ISetEnableWire<T>> where T : new()
{
}

public class SetEnableWireGroup<T> : ISetEnableWireGroup<T> where T : new()
{
    private readonly ISetEnableWire<T>[] _wires;

    public SetEnableWireGroup(ISetEnableWire<T>[] wires)
    {
        _wires = wires;
    }
    
    public IEnumerator<ISetEnableWire<T>> GetEnumerator()
    {
        return ((IEnumerable<ISetEnableWire<T>>)_wires).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _wires.Length;

    public ISetEnableWire<T> this[int index] => _wires[index];
}
