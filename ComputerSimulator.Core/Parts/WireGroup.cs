using System.Collections;
using ComputerSimulator.Core.Enums;

namespace ComputerSimulator.Core.Parts;

public interface IWireGroup : IResettable
{
}

public interface IWireGroup<T> : IWireGroup, IReadOnlyList<IWire<T>> where T : new()
{
    IWire<T> this[OpCode index] { get; }

    void SetValue(T[] values);

    int FindIndex(Predicate<IWire<T>> predicate);
}

public class WireGroup<T> : IWireGroup<T> where T : new()
{
    protected readonly IWire<T>[] Wires;

    public WireGroup(IWire<T>[] wires)
    {
        Wires = wires;
    }

    public int Count => Wires.Length;
     
    public IEnumerator<IWire<T>> GetEnumerator()
    {
        return ((IEnumerable<IWire<T>>)Wires).GetEnumerator();
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

    public int FindIndex(Predicate<IWire<T>> predicate)
    {
        for (var i = 0; i < Wires.Length; i++)
        {
            if (predicate.Invoke(Wires[i]))
            {
                return i;
            }
        }

        return -1;
    }

    public void Reset()
    {
        for (var i = 0; i < Wires.Length; i++)
        {
            if (Wires[i] is IResettable resettable)
            {
                resettable.Reset();
            }
        }
    }
}
