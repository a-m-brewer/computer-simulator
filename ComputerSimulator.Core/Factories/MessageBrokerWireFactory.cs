using Avoid.MessageBroker;
using ComputerSimulator.Core.Models;
using ComputerSimulator.Core.Parts;

namespace ComputerSimulator.Core.Factories;

public class MessageBrokerWireFactory : IWire2Factory
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

    public IWireGroup<T> CreateGroup<T>(string label, T initialValue)
    {
        var group = new WireGroup<T>();

        for (var i = 0; i < _computerSettings.WordSize; i++)
        {
            group.SetWire(i, Create($"{label}-{i}", initialValue));
        }

        return group;
    }
}