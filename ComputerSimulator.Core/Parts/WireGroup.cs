using System.Collections;

namespace ComputerSimulator.Core.Parts;

public interface IWireGroup
{
    event EventHandler<int> WireValuesChanged;
}

public interface IWireGroup<T> : IWireGroup, IReadOnlyList<IWire2<T>>
{
}

public class WireGroup<T> : IWireGroup<T>
{
    private readonly IList<IWire2<T>> _wires;

    public WireGroup(IList<IWire2<T>> wires)
    {
        _wires = wires;

        for (var i = 0; i < _wires.Count; i++)
        {
            var i1 = i;
            _wires[i].SubscribeToValueChanged((sender, _) => WireValuesChanged?.Invoke(sender, i1));
        }
    }

    public IWire2<T> GetWire(int index)
    {
        return _wires[index];
    }

    public T GetValue(int index)
    {
        return _wires[index].Value;
    }

    public void SetValue(int index, T value)
    {
        _wires[index].Value = value;
        // WireValuesChanged?.Invoke(this, index);
    }

    public event EventHandler<int>? WireValuesChanged;

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
}

public static class WireGroupHelper
{
    public static IWireGroup<T> SubscribeToWireValuesChanged<T>(this IWireGroup<T> wires, EventHandler<int> handler)
    {
        wires.WireValuesChanged += handler;
        return wires;
    }
}