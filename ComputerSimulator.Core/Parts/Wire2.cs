using System.Collections.Concurrent;
using Avoid.MessageBroker;

namespace ComputerSimulator.Core.Parts;

public interface IWire2<T>
{
    string Label { get; set; }
    T Value { get; set; }

    void ConnectOutput(Guid id, Action<T> action);
    void DisconnectOutput(Guid id);
}

public class MessageBrokerWire<T> : IWire2<T>, IMessageHandler<T>
{
    private readonly IMessageBroker _messageBroker;
    private T _value;
    private string _label = string.Empty;
    private readonly ConcurrentDictionary<Guid, Action<T>> _actions = new();

    public MessageBrokerWire(IMessageBroker messageBroker, T initialValue)
    {
        _messageBroker = messageBroker;
        _value = initialValue;
    }

    public string Label
    {
        get => _label;
        set
        {
            if (_label == value)
            {
                return;
            }
            
            _messageBroker.ReplaceQueue<bool>(_label, value);
            _messageBroker.ReplaceHandler(_label, value, this);
            _label = value;
        }
    }

    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            ValueChanged();
        }
    }

    public void ConnectOutput(Guid id, Action<T> action)
    {
        _actions[id] = action;
    }

    public void DisconnectOutput(Guid id)
    {
        _actions.TryRemove(id, out _);
    }

    private void ValueChanged()
    {
        if (string.IsNullOrWhiteSpace(Label))
        {
            return;
        }
        
        _messageBroker.Publish(Label, Value);
    }

    public void Handle(T message)
    {
        foreach (var action in _actions.Values)
        {
            action.Invoke(message);
        }
    }
}

public class DisconnectedWire<T> : IWire2<T>
{
    public string Label { get; set; } = string.Empty;
    public T Value { get; set; } = default!;
    
    public void ConnectOutput(Guid guid, Action<T> action)
    {
    }

    public void DisconnectOutput(Guid id)
    {
    }

    public static IWire2<T> Instance => new DisconnectedWire<T>();
}

public static class WireHelper
{
    public static void SetWire<T>(ref IWire2<T> wire, IWire2<T> newValue, Guid componentId, Action<T> action)
    {
        wire.DisconnectOutput(componentId);
        wire = newValue;
        wire.ConnectOutput(componentId, action);
    }
}