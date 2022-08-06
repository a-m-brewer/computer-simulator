using System.Collections.Concurrent;

namespace ComputerSimulator.Core.Parts;

public interface IWireGroup<T>
{
    public Guid Id { get; }
    public void SetWire(int index, IWire2<T> wire);
    public void ConnectOutput(Guid id, Action<IEnumerable<T>> action);
    public void DisconnectOutput(Guid id);
}

public class WireGroup<T> : IWireGroup<T>
{
    private ConcurrentDictionary<Guid, Action<IEnumerable<T>>> _actions = new();
    private ConcurrentDictionary<int, IWire2<T>> _wires = new();

    public Guid Id { get; } = Guid.NewGuid();

    public void SetWire(int index, IWire2<T> wire)
    {
        void HandleInternal(T value)
        {
            HandleValueChanged();
        }
        
        if (_wires.TryRemove(index, out var oldWire))
        {
            oldWire.DisconnectOutput(Id);
        }

        wire.ConnectOutput(Id, HandleInternal);
        _wires[index] = wire;
    }

    private void HandleValueChanged()
    {
        foreach (var action in _actions.Values)
        {
            action.Invoke(_wires.Values.Select(s => s.Value));
        }
    }

    public void ConnectOutput(Guid id, Action<IEnumerable<T>> action)
    {
        _actions[id] = action;
    }

    public void DisconnectOutput(Guid id)
    {
        _actions.TryRemove(id, out _);
    }
}

public class DisconnectedWireGroup<T> : IWireGroup<T>
{
    public static IWireGroup<T> Instance => new DisconnectedWireGroup<T>();
    public Guid Id { get; } = Guid.NewGuid();

    public void SetWire(int index, IWire2<T> wire)
    {
    }

    public void ConnectOutput(Guid id, Action<IEnumerable<T>> action)
    {
    }

    public void DisconnectOutput(Guid id)
    {
    }
}

public static class WireGroupHelper
{
    public static void SetWireGroup<T>(ref IWireGroup<T> wireGroup, IWireGroup<T> newValue, Guid componentId, Action<IEnumerable<T>> action)
    {
        wireGroup.DisconnectOutput(componentId);
        wireGroup = newValue;
        wireGroup.ConnectOutput(componentId, action);
    }
}