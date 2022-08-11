using Avoid.MessageBroker;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public class MessageBrokerWireFactory : IWire2Factory2
{
    private readonly ComputerSettings _computerSettings;
    private readonly IMessageBroker _messageBroker;

    public MessageBrokerWireFactory(
        ComputerSettings computerSettings,
        IMessageBroker messageBroker)
    {
        _computerSettings = computerSettings;
        _messageBroker = messageBroker;
    }
    
    public IWire2<T> Create<T>(string label, T initialValue)
    {
        return new MessageBrokerWire<T>(_messageBroker, initialValue)
        {
            Label = label
        };
    }

    public IWireGroup<T> CreateGroup<T>(string label)
    {
        return new WireGroup<T>(label);
    }

    public IWireGroup<T> CreateGroup<T>(string label, T initialValue)
    {
        return CreateGroup(label, initialValue, _computerSettings.WordSize);
    }

    public IWireGroup<T> CreateGroup<T>(string label, T initialValue, int size)
    {
        var group = new WireGroup<T>(label);

        for (var i = 0; i < size; i++)
        {
            group.SetWire(i, Create($"{label}-{i}", initialValue));
        }

        return group;
    }
}