using Avoid.MessageBroker;

namespace ComputerSimulator.Core.Parts;

public class Wire2
{
    private readonly IMessageBroker _messageBroker;
    private bool _firstValueSet;
    private bool _value;

    public Wire2(
        string label,
        IMessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
        Label = label;
        
        _messageBroker.AddQueue(new MessageQueue<bool>(label));
    }

    public string Label { get; }
    
    public void SetValue(bool value)
    {
        if (_firstValueSet)
        {
            if (_value == value)
            {
                return;
            }
        }

        _value = value;
        _firstValueSet = true;
        
        _messageBroker.Publish(Label, _value);
    }
}