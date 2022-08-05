using Avoid.MessageBroker;

namespace ComputerSimulator.Core.Parts;

public interface IWire2
{
    string Label { get; set; }
    bool Value { get; set; }

    void ConnectOutput(Action<bool> action);
}

public class MessageBrokerWire : IWire2, IMessageHandler<bool>
{
    private readonly IMessageBroker _messageBroker;
    private bool _value;
    private string _label = string.Empty;
    private readonly List<Action<bool>> _actions = new();

    public MessageBrokerWire(IMessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
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

    public bool Value
    {
        get => _value;
        set
        {
            _value = value;
            ValueChanged();
        }
    }

    public void ConnectOutput(Action<bool> action)
    {
        _actions.Add(action);
    }

    private void ValueChanged()
    {
        if (string.IsNullOrWhiteSpace(Label))
        {
            return;
        }
        
        _messageBroker.Publish(Label, Value);
    }

    public void Handle(bool message)
    {
        foreach (var action in _actions)
        {
            action.Invoke(message);
        }
    }
}

public class DisconnectedWire : IWire2
{
    public string Label { get; set; } = string.Empty;
    public bool Value { get; set; }
    public void ConnectOutput(Action<bool> action)
    {
    }

    public static IWire2 Instance => new DisconnectedWire();
}