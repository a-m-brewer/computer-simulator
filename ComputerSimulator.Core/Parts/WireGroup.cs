using System.Collections;
using System.Collections.Concurrent;
using ComputerSimulator.Core.Events;

namespace ComputerSimulator.Core.Parts;

public interface IWireGroup
{
    event EventHandler<int> WireValuesChanged;
    Guid Id { get; }
    string Label { get; }
}

public interface IWireGroup<T> : IWireGroup, IReadOnlyList<IWire2<T>>
{
    event EventHandler<WireGroupWireChangedEventArgs<T>> WireChanged;
    public void SetWire(int index, IWire2<T> wire);
}

public class WireGroup<T> : IWireGroup<T>
{
    protected readonly ConcurrentDictionary<int, IWire2<T>> Wires = new();

    public WireGroup(string label)
    {
        Label = label;
    }

    public event EventHandler<WireGroupWireChangedEventArgs<T>>? WireChanged;
    public event EventHandler<int>? WireValuesChanged;
    public Guid Id { get; } = Guid.NewGuid();
    public string Label { get; }

    public void SetWire(int index, IWire2<T> wire)
    {
        var wireChangedEventArgs = new WireGroupWireChangedEventArgs<T>(index, wire);
        
        void HandleInternal(object? sender, EventArgs e)
        {
            HandleValueChanged(index);
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

    private void HandleValueChanged(int index)
    {
        WireValuesChanged?.Invoke(this, index);
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
    public event EventHandler<int>? WireValuesChanged;

    public Guid Id { get; } = Guid.NewGuid();
    public string Label => string.Empty;

    public void SetWire(int index, IWire2<T> wire)
    {
        throw new NotImplementedException();
    }

    public void ConnectOutputs(Guid id, Action<IEnumerable<T>> action)
    {
        throw new NotImplementedException();
    }

    public void DisconnectOutputs(Guid id)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<IWire2<T>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => 0;

    public IWire2<T> this[int index] => throw new NotImplementedException();
}

public static class WireGroupHelper
{
    public static void ReSubscribeWireValuesChanged<T>(
        IWireGroup<T> wireGroup, 
        IWireGroup<T> newValue,
        EventHandler<int> eventHandler)
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