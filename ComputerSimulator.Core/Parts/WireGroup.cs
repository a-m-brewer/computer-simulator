using System.Collections;
using ComputerSimulator.Core.Enums;

namespace ComputerSimulator.Core.Parts;

public interface IWireGroup
{
}

public interface IWireGroup<T> : IWireGroup, IReadOnlyList<IWire<T>>
{
    IWire<T> this[OpCode index] { get; }

    void SetValue(T[] values);
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
    public void SetValue(T[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            Wires[i].Value = values[i];
        }
    }
}