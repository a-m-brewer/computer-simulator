namespace ComputerSimulator.Core.Parts;

public interface IWire2
{
    event EventHandler ValueChanged;
}

public interface IWire2<T> : IWire2
{
    T Value { get; set; }
}

// public class MessageBrokerWire<T> : IWire2<T>, IMessageHandler<T>
// {
//     private bool _valueSet;
//     private readonly IMessageBroker _messageBroker;
//     private T _value;
//     private string _label = string.Empty;
//
//     public MessageBrokerWire(IMessageBroker messageBroker, T initialValue)
//     {
//         _messageBroker = messageBroker;
//         _value = initialValue;
//     }
//
//     public event EventHandler? ValueChanged;
//
//     public string Label
//     {
//         get => _label;
//         set
//         {
//             if (_label == value)
//             {
//                 return;
//             }
//             
//             _messageBroker.ReplaceQueue<bool>(_label, value);
//             _messageBroker.ReplaceHandler(_label, value, this);
//             _label = value;
//         }
//     }
//
//     public T Value
//     {
//         get => _value;
//         set
//         {
//             if (_valueSet && EqualityComparer<T>.Default.Equals(_value, value))
//             {
//                 return;
//             }
//
//             _valueSet = true;
//             _value = value;
//             InternalValueChanged();
//         }
//     }
//
//     private void InternalValueChanged()
//     {
//         if (string.IsNullOrWhiteSpace(Label))
//         {
//             return;
//         }
//         
//         _messageBroker.Publish(Label, Value);
//     }
//
//     public void Handle(T message)
//     {
//         ValueChanged?.Invoke(this, EventArgs.Empty);
//     }
// }

public class EventWire<T> : IWire2<T>
{
    private T _value;
    private bool _valueSet;

    public EventWire(T value)
    {
        _value = value;
    }

    public event EventHandler? ValueChanged;

    public T Value
    {
        get => _value;
        set
        {
            if (_valueSet && EqualityComparer<T>.Default.Equals(_value, value))
            {
                return;
            }

            _valueSet = true;
            _value = value;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

public class DisconnectedWire<T> : IWire2<T>
{
    public event EventHandler? ValueChanged;

    public string Label { get; set; } = string.Empty;

    public T Value
    {
        get => default!;
        set => throw new NotImplementedException();
    }

    public static IWire2<T> Instance => new DisconnectedWire<T>();
}

public static class WireHelper
{
    public static IWire2<T> SubscribeToValueChanged<T>(this IWire2<T> wire, EventHandler handler)
    {
        wire.ValueChanged += handler;
        return wire;
    }
}