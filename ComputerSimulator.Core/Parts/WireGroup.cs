using System.Collections;
using System.Collections.Concurrent;
using ComputerSimulator.Core.Events;

namespace ComputerSimulator.Core.Parts;

public interface IWireGroup<T> : IReadOnlyList<IWire2<T>>
{
    event EventHandler<WireGroupWireChangedEventArgs<T>> WireChanged;
    event EventHandler WireValuesChanged;
    public Guid Id { get; }
    public void SetWire(int index, IWire2<T> wire);
}

public class WireGroup<T> : IWireGroup<T>
{
    protected readonly ConcurrentDictionary<int, IWire2<T>> Wires = new();

    public event EventHandler<WireGroupWireChangedEventArgs<T>>? WireChanged;
    public event EventHandler? WireValuesChanged;
    public Guid Id { get; } = Guid.NewGuid();

    public void SetWire(int index, IWire2<T> wire)
    {
        var wireChangedEventArgs = new WireGroupWireChangedEventArgs<T>(index, wire);
        
        void HandleInternal(object? sender, EventArgs e)
        {
            HandleValueChanged();
        }
        
        if (Wires.TryRemove(index, out var oldWire))
        {
            wireChangedEventArgs.OldWire = oldWire;
            oldWire.ValueChanged -= HandleInternal;
        }

        wire.ValueChanged += HandleInternal;
        Wires[index] = wire;
        WireChanged?.Invoke(this, wireChangedEventArgs);
    }

    private void HandleValueChanged()
    {
        WireValuesChanged?.Invoke(this, EventArgs.Empty);
    }

    public IEnumerator<IWire2<T>> GetEnumerator()
    {
        return Wires.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => Wires.Count;

    public IWire2<T> this[int index] => Wires[index];
}

public class DisconnectedWireGroup<T> : IWireGroup<T>
{
    public static IWireGroup<T> Instance => new DisconnectedWireGroup<T>();
    public event EventHandler<WireGroupWireChangedEventArgs<T>>? WireChanged;
    public event EventHandler? WireValuesChanged;

    public Guid Id { get; } = Guid.NewGuid();

    public void SetWire(int index, IWire2<T> wire)
    {
    }

    public void ConnectOutputs(Guid id, Action<IEnumerable<T>> action)
    {
    }

    public void DisconnectOutputs(Guid id)
    {
    }

    public IEnumerator<IWire2<T>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count { get; }

    public IWire2<T> this[int index] => throw new NotImplementedException();
}

public static class WireGroupHelper
{
    public static void ReSubscribeWireValuesChanged<T>(
        IWireGroup<T> wireGroup, 
        IWireGroup<T> newValue,
        EventHandler eventHandler)
    {
        wireGroup.WireValuesChanged -= eventHandler;
        newValue.WireValuesChanged += eventHandler;
    }
    
    public static void ReSubscribeWireChanged<T>(
        IWireGroup<T> wireGroup, 
        IWireGroup<T> newValue,
        EventHandler<WireGroupWireChangedEventArgs<T>> action)
    {
        wireGroup.WireChanged -= action;
        newValue.WireChanged += action;
    }
}